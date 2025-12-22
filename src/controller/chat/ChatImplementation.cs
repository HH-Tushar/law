
using api.Data;
using ChatApp.Models;
using ChatController.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ChatController.Implementation
{
    public class ChatControllerImplementation : ChatControllerAbstraction
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<ChatControllerImplementation> _logger;

        public ChatControllerImplementation(DatabaseContext context, ILogger<ChatControllerImplementation> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task<Attempt<ChatConversation>> CreateConversationAsync(
            CreateConversationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return Attempt<ChatConversation>.Failed(
                        new Failure("Validation Error", "UserId is required.", 400));
                }

                if (string.IsNullOrWhiteSpace(request.InitialMessage))
                {
                    return Attempt<ChatConversation>.Failed(
                        new Failure("Validation Error", "Initial message is required.", 400));
                }


                //call openapi for the initial message.

                // --- Generate title automatically if not provided ---
                var title = string.IsNullOrWhiteSpace(request.Title)
                    ? GenerateTitleFromMessage(request.InitialMessage)
                    : request.Title;


                var conversation = new ChatConversation
                {
                    UserId = request.UserId,
                    Title = string.IsNullOrWhiteSpace(request.Title) ? "New Conversation" : request.Title,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                };
                _context.ChatConversations.Add(conversation);
                // Add initial message
                var message = new ChatMessage
                {
                    ConversationId = conversation.Id,
                    Sender = SenderType.User,
                    Query = request.InitialMessage,
                    Timestamp = DateTime.UtcNow
                };




                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();

                return Attempt<ChatConversation>.Success(conversation);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CREATE CONVERSATION ERROR] {ex}");
                _logger.LogError(ex, "Error creating conversation");

                return Attempt<ChatConversation>.Failed(
                    new Failure("Error", "An error occurred while creating the conversation.", 500));
            }

        }





public override async Task<Attempt<PagedResult<ChatMessage>>> GetMessagesAsync(
    string conversationId, int page = 1, int pageSize = 20)
{
    try
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return Attempt<PagedResult<ChatMessage>>.Failed(
                new Failure("Validation Error", "ConversationId is required.", 400));
        }

        // 1️⃣ Get total count first
        var totalCount = await _context.ChatMessages
            .Where(m => m.ConversationId == conversationId)
            .CountAsync();

        // 2️⃣ Fetch paginated messages
        var messages = await _context.ChatMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 3️⃣ Wrap in PagedResult
        var pagedResult = new PagedResult<ChatMessage>
        {
            Data = messages,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Attempt<PagedResult<ChatMessage>>.Success(pagedResult);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching messages for conversation {ConversationId}", conversationId);
        return Attempt<PagedResult<ChatMessage>>.Failed(
            new Failure("Error", "An error occurred while retrieving messages.", 500));
    }
}




        // ---------------- Add a single message ----------------
        public override async Task<Attempt<ChatMessage>> AddMessageAsync(ChatMessage message)
        {
            try
            {
                _context.ChatMessages.Add(message);

                // Update conversation metadata
                var conversation = await _context.ChatConversations.FirstOrDefaultAsync(c => c.Id == message.ConversationId);
                if (conversation != null)
                {
                    conversation.LastUpdated = DateTime.UtcNow;

                }

                await _context.SaveChangesAsync();

                return Attempt<ChatMessage>.Success(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving message for conversation {ConversationId}", message.ConversationId);
                return Attempt<ChatMessage>.Failed(new Failure("Error", "Failed to save message", 500));
            }
        }




        // ------------------ Helper Method ------------------
        private string GenerateTitleFromMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return "New Conversation";

            var words = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 5) return message;

            return string.Join(' ', words.Take(5)) + "...";
        }



    }
}
