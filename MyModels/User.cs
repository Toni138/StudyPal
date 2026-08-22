using System.ComponentModel.DataAnnotations;

namespace MyModels
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string Username { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsEmailVerified { get; set; } = false;
        public int FailedLoginAttempts { get; set; } = 0;
        public UserStats UserStats { get; set; }
        public ICollection<StudySession> StudySessions { get; set; }

    }
}

