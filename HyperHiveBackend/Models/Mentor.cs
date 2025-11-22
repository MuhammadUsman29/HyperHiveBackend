namespace HyperHiveBackend.Models
{
    public class Mentor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string Department { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public bool IsAvailable { get; set; } = true;
        
        // JSON column to store AI-relevant mentor data
        // This will contain: expertise areas, availability, mentoring style, etc.
        public string? AIProfileData { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

