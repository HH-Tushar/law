using Microsoft.AspNetCore.Mvc;
using UserProfile.Abstract;

namespace UserProfile.Implements
{
    public class UserProfileImplementation : UserProfileAbstraction
    {
        public override async Task<IActionResult> GetAllUser()
        {
            // Example DB operation
            await Task.Delay(100);

            var products = new[]
            {
                new { Id = 1, Name = "Laptop", Price = 1200 },
                new { Id = 2, Name = "Phone", Price = 800 }
            };

            return Ok(products);
        }
    }
}