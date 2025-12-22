using Microsoft.AspNetCore.SignalR;
using ChatApp.Models;
using ChatController.Abstract;
using System.Text.Json;
using System.Text;
using LowerCase;

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

    public async Task SendMessage(SendMessageRequest content)
    {
        if (string.IsNullOrWhiteSpace(content.ConversationId))
        {
            await Clients.Caller.SendAsync("MessageSendFailed", "Invalid conversation");
            return;
        }

        if (string.IsNullOrWhiteSpace(content.Query))
        {
            await Clients.Caller.SendAsync("MessageSendFailed", "Message cannot be empty");
            return;
        }

        // 1️⃣ Send user message
        var userMessage = new ChatMessage
        {
            ConversationId = content.ConversationId,
            Sender = SenderType.User,
            Query = content.Query,
            Timestamp = DateTime.UtcNow
        };

        await Clients.Group(content.ConversationId)
            .SendAsync("ReceiveMessage", userMessage);

        try
        {

            var AiChatResponse = await CallAiApiAsync(content: content);

            // 2️⃣ AI status: thinking...
            await Clients.Group(content.ConversationId).SendAsync(
                "ReceiveMessage",
                new ChatMessage
                {
                    ConversationId = content.ConversationId,
                    Sender = SenderType.Assistant,
                    Query = "AI is thinking...",
                    Timestamp = DateTime.UtcNow
                }
            );

            // await Task.Delay(TimeSpan.FromSeconds(2));

            // 3️⃣ AI status: working...
            await Clients.Group(content.ConversationId).SendAsync(
                "ReceiveMessage",
                new ChatMessage
                {
                    ConversationId = content.ConversationId,
                    Sender = SenderType.Assistant,
                    Query = "Working deeply on your data...",
                    Timestamp = DateTime.UtcNow
                }
            );



            // 4️⃣ Final AI response
            var aiMessage = new ChatMessage
            {
                ConversationId = content.ConversationId,
                Sender = SenderType.Assistant,
                Query = "This is a dummy AI response. Replace with OpenAI later.",
                Timestamp = DateTime.UtcNow
            };
 Console.WriteLine($"AI response ready to send in hub    :    {AiChatResponse.Response}");
            await Clients.Group(content.ConversationId)
                .SendAsync("ReceiveMessage", AiChatResponse);
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


    private async Task<AiChatResponse> CallAiApiAsync(
       SendMessageRequest content)
    {
        try
        {
            using var httpClient = new HttpClient();


            var json = JsonSerializer.Serialize(
         content,
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = new LowerCaseNamingPolicy()
        }
    );

            Console.Write(json);
            using var httpContent = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var httpResponse = await httpClient.PostAsync(
                "https://c11dea5a4b35.ngrok-free.app/chat",
                httpContent
            );
            Console.Write(httpResponse);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"AI API returned status {httpResponse.StatusCode}"
                );
            }

            var responseJson = await httpResponse.Content.ReadAsStringAsync();
         
            var aiResponse = JsonSerializer.Deserialize<AiChatResponse>(
                responseJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = new CamelCaseNamingPolicy()
                }
            );
            Console.Write("aiResponse = = = = ");
            Console.Write(aiResponse.Response);
            if (aiResponse == null)
            {
                throw new Exception("AI response deserialization failed");
            }

            return aiResponse;
        }
        catch (TaskCanceledException ex)
        {
            // Timeout
            Console.WriteLine($"AI API timeout: {ex.Message}");
            return new AiChatResponse
            {
                Response = "AI service timed out. Please try again later.",
                Sources = new List<AiSource>()
            };
        }
        catch (HttpRequestException ex)
        {
            // Network / 4xx / 5xx
            Console.WriteLine($"AI API HTTP error: {ex.Message}");
            return new AiChatResponse
            {
                Response = "AI service is currently unavailable.",
                Sources = new List<AiSource>()
            };
        }
        catch (Exception ex)
        {
            // Unknown error
            Console.WriteLine($"AI API unexpected error: {ex.Message}");
            return new AiChatResponse
            {
                Response = "An unexpected error occurred while generating AI response.",
                Sources = new List<AiSource>()
            };
        }
    }

}
