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

        public string Content { get; set; } = string.Empty; // Text content

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false; // Optional for UI

        // Optional fields for future use:
        public string? Metadata { get; set; } // JSON string for extra info
        public string? AttachmentUrl { get; set; } // image/audio/file
    }
}
