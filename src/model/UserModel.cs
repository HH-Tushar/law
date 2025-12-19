



namespace UserProfileModel
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // Core Identity
        public string Name { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public required UserProfession Profession { get; set; }
        public required AccountCreatedBy AccountCreatedBy { get; set; }

        // Metadata
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }


    }

}














public enum UserRole
{
    StandardUser,
    Moderator,
    Admin,
    User,
}
public enum AccountCreatedBy
{
    EmailPassword,
    Google,
    Apple,

}
public enum UserProfession
{
    Student,
    Lawer,
    Public,
    Unknown
}