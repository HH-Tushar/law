using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserProfile.Abstract;
using UserProfile.Implements;


namespace UserProfile.Api
{
    [ApiController]
    [Route("api/user")]
    [Authorize] // 🔐 JWT validation happens here
    public class ProductApi : ControllerBase
    {

 private readonly UserProfileAbstraction _productController;

        public ProductApi(UserProfileAbstraction productController)
        {
            _productController = productController;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            // Token is already validated by middleware
            return await _productController.GetAllUser();
        }

     
    }
}
