using System;

namespace ChatApp.Models
{
    public enum SenderType
    {
        User,
        Assistant, // AI / bot
        System     // Optional: system messages
    }

    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Unique message ID

        public string ConversationId { get; set; } = string.Empty; // Conversation grouping

        public SenderType Sender { get; set; }

        public string Query { get; set; } = string.Empty; // Text content

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false; // Optional for UI

        // Optional fields for future use:
        public string? Metadata { get; set; } // JSON string for extra info
        public string? AttachmentUrl { get; set; } // image/audio/file
    }


public class ChatHistoryItem
{
    public required string Role { get; set; }     // "user" | "assistant"
    public required string Query { get; set; }
}

public class SendMessageRequest
{
    public required string ConversationId { get; set; }
    // public List<ChatHistoryItem> History { get; set; }=[];
    public required string  Query { get; set; } // latest user message (optional but useful)
}





public class AiChatResponse
{
    public required string  Response { get; set; }
    public List<AiSource> Sources { get; set; }=[];
}

public class AiSource
{
    public string Act_Name { get; set; }=string.Empty;
    public string Section_Name { get; set; }=string.Empty;
    public string Act_No { get; set; }=string.Empty;
    public double Score { get; set; }=0;
}


}
