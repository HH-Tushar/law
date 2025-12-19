
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class ChatConversation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Unique conversation ID

        public string UserId { get; set; } = string.Empty; // Owner of conversation

        public string Title { get; set; } = "New Conversation"; // Optional user-defined title

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;


        // Optional: flag for archived conversations
        public bool IsArchived { get; set; } = false;
    }




    public class CreateConversationRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty; // Owner of the conversation
        [Required]
        public string InitialMessage { get; set; } = string.Empty;
        public string Title { get; set; } = "New Conversation"; // Optional
    }





}
