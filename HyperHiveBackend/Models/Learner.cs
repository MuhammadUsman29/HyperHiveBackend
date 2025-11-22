namespace HyperHiveBackend.Models
{
    public class Learner
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; }
        public string? Bio { get; set; }
        
        // JSON column to store AI-relevant profile data
        // This will contain: skills, interests, goals, learning preferences, etc.
        public string? AIProfileData { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

