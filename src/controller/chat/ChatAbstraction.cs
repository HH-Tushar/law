using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatController.Abstract
{
    public abstract class ChatControllerAbstraction : ControllerBase
    {

        public abstract Task<Attempt<ChatConversation>> CreateConversationAsync(
          CreateConversationRequest request);

        public abstract Task<Attempt<ChatMessage>> AddMessageAsync(ChatMessage message);
       public abstract Task<Attempt<PagedResult<ChatMessage>>> GetMessagesAsync(
    string conversationId, int page = 1, int pageSize = 20);






    }
}



