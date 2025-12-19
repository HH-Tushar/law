using ChatController.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserController.Abstract;



namespace UserProfile.Api
{
    [ApiController]
    [Route("api/user")]
    // [Authorize] // 🔐 JWT validation happens here
    public class ProductApi : ControllerBase
    {

        private readonly UserProfileAbstraction _userController;
        private readonly ChatControllerAbstraction _chatController;

        public ProductApi(UserProfileAbstraction userController,ChatControllerAbstraction chatController)
        {
            _userController = userController;
            _chatController = chatController;
        }

        // GET api/user/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // need to check if asking for own or asked by admin 

            var result = await _userController.GetUserByIdAsync(id);

            return result.IsSuccess
                ? Ok(result.Data)
                : BadRequest(result.Error);
        }






        // POST api/user/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] UserRegistrationRequest request)
        {
            var result = await _userController.RegisterAsync(request);

            return result.IsSuccess
                ? Ok(result.Data)
                : BadRequest(result.Error);
        }

        // POST api/user/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var result = await _userController.LoginAsync(
                request.Email,
                request.Password);

            return result.IsSuccess
                ? Ok(result.Data)
                : BadRequest(result.Error);
        }




        // POST api/user/login/google
        [HttpPost("login/google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin(
            [FromBody] GoogleLoginRequest request)
        {
            var result = await _userController.LoginWithGoogleAsync(request);

            return result.IsSuccess
                ? Ok(result.Data)
                : BadRequest(result.Error);
        }




        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(string conversationId, int page = 1, int pageSize = 20)
        {
            var result = await _chatController.GetMessagesAsync(conversationId, page, pageSize);

            return result.IsSuccess
                ? Ok(result.Data)
                : BadRequest(result.Error);
        }




    }
}
