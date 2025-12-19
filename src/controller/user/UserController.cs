using Microsoft.AspNetCore.Mvc;

namespace UserController.Abstract
{
    public abstract class UserProfileAbstraction : ControllerBase
    {
        public abstract Task<Attempt<PagedResult<UserProfileResponse>>> GetUsersPaginatior(int page, int pageSize);
        public abstract Task<Attempt<UserProfileResponse>> GetUserByIdAsync(Guid id);


        public abstract Task<Attempt<UserProfileResponse>> RegisterAsync(
                  UserRegistrationRequest request
              );

        public abstract Task<Attempt<UserProfileResponse>> LoginAsync(
            string email,
            string password
        );

          public abstract Task<Attempt<UserProfileResponse>> LoginWithGoogleAsync(
            GoogleLoginRequest request);


    }
}



