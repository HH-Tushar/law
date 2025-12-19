
using System.ComponentModel.DataAnnotations;
using api.Data;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using UserController.Abstract;
using UserProfileModel;


namespace UserController.Implements
{
    public class UserProfileService(DatabaseContext context, ILogger<UserProfileService> logger) : UserProfileAbstraction
    {
        private readonly DatabaseContext _context = context;
        private readonly ILogger<UserProfileService> _logger=logger;

        // 1. Implementation for Paginated List
        public override async Task<Attempt<PagedResult<UserProfileResponse>>> GetUsersPaginatior(int page, int pageSize)
        {
            try
            {
                var query = _context.Users.AsNoTracking();

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(u => u.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => MapToResponse(u))
                    .ToListAsync();

                var result = new PagedResult<UserProfileResponse>
                {
                    Data = items,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                return Attempt<PagedResult<UserProfileResponse>>.Success(result);
            }
            catch (Exception ex)
            {

                // 1️⃣ Console (quick debugging)
                Console.WriteLine($"[GetUsersAsync ERROR] {ex}");



                return Attempt<PagedResult<UserProfileResponse>>.Failed(
                    new Failure("Fetch Error", "An error occurred while retrieving users.", 500));
            }
        }











        public override async Task<Attempt<UserProfileResponse>> GetUserByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Attempt<UserProfileResponse>.Failed(
                        new Failure(
                            title: "Invalid Id",
                           code: 400
                        )
                    );
                }

                var user = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == id)
                    .Select(u => MapToResponse(u))
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return Attempt<UserProfileResponse>.Failed(
                        new Failure(
                            title: "Not Found",
                            code: 404
                        )
                    );
                }

                return Attempt<UserProfileResponse>.Success(user);
            }
            catch (Exception ex)
            {
                // 🔴 Log exception
                Console.WriteLine($"[GetUserByIdAsync ERROR] {ex}");

                return Attempt<UserProfileResponse>.Failed(
                    new Failure(
                        title: "Fetch Error",
                        code: 500
                    )
                );
            }
        }












        // ---------------- REGISTER ----------------
        public override async Task<Attempt<UserProfileResponse>> RegisterAsync(
            UserRegistrationRequest request)
        {
            try
            {
                // ✅ Manual validation using DataAnnotations
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);

                if (!Validator.TryValidateObject(
                        request,
                        validationContext,
                        validationResults,
                        validateAllProperties: true))
                {
                    var errorMessage = string.Join(
                        " | ",
                        validationResults.Select(v => v.ErrorMessage)
                    );

                    return Attempt<UserProfileResponse>.Failed(
                        new Failure(
                            "Validation Failed",
                            errorMessage,
                            400
                        )
                    );
                }

                // ✅ Check email uniqueness
                var exists = await _context.Users
                    .AnyAsync(u => u.Email == request.Email);

                if (exists)
                {
                    return Attempt<UserProfileResponse>.Failed(
                        new Failure(
                            "Registration Failed",
                            "Email already exists.",
                            409
                        )
                    );
                }

                var user = new UserModel
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email.Trim().ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Name = request.Name.Trim(),
                    Profession = Enum.TryParse<UserProfession>(
                        request.Profession,
                        true,
                        out var profession)
                        ? profession
                        : UserProfession.Unknown,

                    AccountCreatedBy = AccountCreatedBy.EmailPassword,
                    JoinedAt = DateTime.UtcNow,
                    IsOnline = false
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Attempt<UserProfileResponse>.Success(MapToResponse(user));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REGISTER ERROR] {ex}");
                _logger.LogError(ex, "Error during user registration");

                return Attempt<UserProfileResponse>.Failed(
                    new Failure(
                        "Registration Error",
                        "An error occurred during registration.",
                        500
                    )
                );
            }
        }








        // ---------------- LOGIN ----------------
        public override async Task<Attempt<UserProfileResponse>> LoginAsync(
            string email,
            string password)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null ||
                    !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    return Attempt<UserProfileResponse>.Failed(
                        new Failure(
                            "Login Failed",
                            "Invalid email or password.",
                            401
                        )
                    );
                }

                user.IsOnline = true;
                user.LastSeen = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Attempt<UserProfileResponse>.Success(MapToResponse(user));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOGIN ERROR] {ex}");
                _logger.LogError(ex, "Error during login");

                return Attempt<UserProfileResponse>.Failed(
                    new Failure(
                        "Login Error",
                        "An error occurred during login.",
                        500
                    )
                );
            }
        }





        public override async Task<Attempt<UserProfileResponse>> LoginWithGoogleAsync(
            GoogleLoginRequest request)
        {
            try
            {
                // 🔐 Validate Google token
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    request.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[]
                        {
                    "YOUR_GOOGLE_CLIENT_ID"
                        }
                    });

                var email = payload.Email;
                var name = payload.Name;

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                // 🆕 First-time Google login → create user
                if (user == null)
                {
                    user = new UserModel
                    {
                        Id = Guid.NewGuid(),
                        Email = email,
                        Name = name,
                        Role = UserRole.User,
                        Profession = UserProfession.Unknown,
                        JoinedAt = DateTime.UtcNow,
                        IsOnline = true,
                        LastSeen = DateTime.UtcNow,
                        AccountCreatedBy = AccountCreatedBy.Google,
                        // 🚫 No password for Google users
                        PasswordHash = string.Empty
                    };

                    _context.Users.Add(user);
                }
                else
                {
                    user.IsOnline = true;
                    user.LastSeen = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Attempt<UserProfileResponse>.Success(MapToResponse(user));
            }
            catch (InvalidJwtException)
            {
                return Attempt<UserProfileResponse>.Failed(
                    new Failure(
                        "Invalid Token",
                        "Google authentication failed.",
                        401
                    )
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GOOGLE LOGIN ERROR] {ex}");
                _logger.LogError(ex, "Error during Google login");

                return Attempt<UserProfileResponse>.Failed(
                    new Failure(
                        "Login Error",
                        "An error occurred during Google login.",
                        500
                    )
                );
            }
        }





        private static UserProfileResponse MapToResponse(UserModel user)
        {
            return new UserProfileResponse
            {
                Id = user.Id,
                Name = user.Name,
                Role = user.Role.ToString(),
                Profession = user.Profession.ToString(),
            };
        }












    }
}