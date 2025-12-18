using Microsoft.AspNetCore.Mvc;

namespace UserProfile.Abstract
{
    public abstract class UserProfileAbstraction: ControllerBase
    {
        public abstract Task<IActionResult> GetAllUser();
    }
}