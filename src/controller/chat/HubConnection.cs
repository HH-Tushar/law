using Microsoft.AspNetCore.SignalR;
using ChatApp.Models;
using ChatController.Abstract;


public class ChatHub : Hub
{
    private readonly ChatControllerAbstraction _conversationService;

    public ChatHub(ChatControllerAbstraction conversationService)
    {
        _conversationService = conversationService;
    }

    // Called when a client connects
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        // if (!string.IsNullOrEmpty(userId))
        // {
        //     await _conversationService.SetUserOnlineStatusAsync(userId, true);
        // }
        await base.OnConnectedAsync();
    }

    // Called when a client disconnects (app closed, network lost)
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        // if (!string.IsNullOrEmpty(userId))
        // {
        //     await _conversationService.SetUserOnlineStatusAsync(userId, false);
        // }
        await base.OnDisconnectedAsync(exception);
    }

    // Join a conversation group
    // public async Task JoinConversation(string conversationId)
    // {
    //     await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    // }




    // Join a conversation by checking identity
    public async Task JoinConversation(string conversationId)
    {
        var userId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(userId))
            throw new HubException("User not authenticated");

        // Get conversation from DB
        // var conversation = await _conversationService.GetConversationByIdAsync(conversationId);
        // if (conversation == null)
        //     throw new HubException("Conversation not found");

        // // Only allow owner
        // if (conversation.UserId != userId)
        //     throw new HubException("You are not allowed to join this conversation");

        // Add connection to conversation group
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }



    // Leave a conversation group
    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }


    public async Task SendMessage(string conversationId, string content)
    {
        var userId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("MessageSendFailed", "User not authenticated");
            return;
        }

        // // 1️⃣ Validate conversation ownership
        // var conversationResult = await _conversationService.GetConversationByIdAsync(conversationId);
        // if (!conversationResult.Success || conversationResult.Data == null)
        // {
        //     await Clients.Caller.SendAsync("MessageSendFailed", "Conversation not found");
        //     return;
        // }

        // if (conversationResult.Data.UserId != userId)
        // {
        //     await Clients.Caller.SendAsync("MessageSendFailed", "You are not allowed to send messages to this conversation");
        //     return;
        // }

        // 2️⃣ Create user message
        var userMessage = new ChatMessage
        {
            ConversationId = conversationId,
            Sender = SenderType.User,
            Content = content,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // 3️⃣ Save user message
            var saveUserResult = await _conversationService.AddMessageAsync(userMessage);
            if (!saveUserResult.IsSuccess)
            {
                await Clients.Caller.SendAsync("MessageSendFailed", saveUserResult?.Error?.Title ?? "Failed to save message");
                return;
            }

            // 4️⃣ Broadcast user message
            await Clients.Group(conversationId).SendAsync("ReceiveMessage", userMessage);

            // 5️⃣ Call OpenAI API
            string aiResponseText;
            try
            {
                // Replace this with your actual OpenAI service call
                // aiResponseText = await OpenAIService.GetResponseAsync(content);
                aiResponseText = "This is a dummy AI response. Replace with OpenAI later.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OpenAI API error: {ex.Message}");
                await Clients.Caller.SendAsync("MessageSendFailed", "AI response failed");
                return;
            }

            // 6️⃣ Save AI assistant message
            var aiMessage = new ChatMessage
            {
                ConversationId = conversationId,
                Sender = SenderType.Assistant,
                Content = aiResponseText,
                Timestamp = DateTime.UtcNow
            };

            var saveAIResult = await _conversationService.AddMessageAsync(aiMessage);
            if (!saveAIResult.IsSuccess)
            {
                await Clients.Caller.SendAsync("MessageSendFailed", saveUserResult?.Error?.Title ?? "Failed to save AI message");
                return;
            }

            // 7️⃣ Broadcast AI message
            await Clients.Group(conversationId).SendAsync("ReceiveMessage", aiMessage);
        }
        catch (Exception ex)
        {
            // 8️⃣ Catch unexpected errors
            Console.WriteLine($"Unexpected error in SendMessage: {ex.Message}");
            await Clients.Caller.SendAsync("MessageSendFailed", "An unexpected error occurred while sending the message");
        }
    }


}




// 5️⃣ Dummy AI response (replace with OpenAI later)
