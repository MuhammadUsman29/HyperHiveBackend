namespace HyperHiveBackend.Models
{
    public class Manager
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public string? Bio { get; set; }
        
        // JSON column to store AI-relevant manager data
        // This will contain: team goals, focus areas, management preferences, etc.
        public string? AIProfileData { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

