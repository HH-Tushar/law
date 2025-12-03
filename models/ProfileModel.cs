
    public class Profile
    {
        public int Id { get; set; }

        // Remove 'required'. 
        // The property must be initialized, typically by the JSON deserializer.
        public string Name { get; set; } = null!; // Use null-forgiving operator if it must be non-null
        public string Email { get; set; } = null!; // Use null-forgiving operator if it must be non-null

        public string? ProfileImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
