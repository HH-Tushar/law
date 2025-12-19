public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    
    // Profile Details
    public string Profession { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    
    // Real-time Chat Info
    public string StatusMessage { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
}