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

    private string GetUserId()
    {
        // Authenticated → UserIdentifier
        // Anonymous → ConnectionId
        return Context.UserIdentifier ?? Context.ConnectionId;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        Console.WriteLine($"User connected: {userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        Console.WriteLine($"User disconnected: {userId}");
        await base.OnDisconnectedAsync(exception);
    }

    // Join a conversation (no auth required)
    public async Task JoinConversation(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new HubException("Invalid conversation id");

        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }

 public async Task SendMessage(string conversationId, string content)
{
    if (string.IsNullOrWhiteSpace(conversationId))
    {
        await Clients.Caller.SendAsync("MessageSendFailed", "Invalid conversation");
        return;
    }

    if (string.IsNullOrWhiteSpace(content))
    {
        await Clients.Caller.SendAsync("MessageSendFailed", "Message cannot be empty");
        return;
    }

    // 1️⃣ Send user message
    var userMessage = new ChatMessage
    {
        ConversationId = conversationId,
        Sender = SenderType.User,
        Content = content,
        Timestamp = DateTime.UtcNow
    };

    await Clients.Group(conversationId)
        .SendAsync("ReceiveMessage", userMessage);

    try
    {
        // 2️⃣ AI status: thinking...
        await Clients.Group(conversationId).SendAsync(
            "ReceiveMessage",
            new ChatMessage
            {
                ConversationId = conversationId,
                Sender = SenderType.Assistant,
                Content = "AI is thinking...",
                Timestamp = DateTime.UtcNow
            }
        );

        await Task.Delay(TimeSpan.FromSeconds(2));

        // 3️⃣ AI status: working...
        await Clients.Group(conversationId).SendAsync(
            "ReceiveMessage",
            new ChatMessage
            {
                ConversationId = conversationId,
                Sender = SenderType.Assistant,
                Content = "Working deeply on your data...",
                Timestamp = DateTime.UtcNow
            }
        );

        await Task.Delay(TimeSpan.FromSeconds(3)); // total delay = 5s

        // 4️⃣ Final AI response
        var aiMessage = new ChatMessage
        {
            ConversationId = conversationId,
            Sender = SenderType.Assistant,
            Content = "This is a dummy AI response. Replace with OpenAI later.",
            Timestamp = DateTime.UtcNow
        };

        await Clients.Group(conversationId)
            .SendAsync("ReceiveMessage", aiMessage);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"AI response error: {ex.Message}");
        await Clients.Caller.SendAsync(
            "MessageSendFailed",
            "An unexpected error occurred while generating AI response"
        );
    }
}


}
