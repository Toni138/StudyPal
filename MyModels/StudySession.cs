using System;
using System.ComponentModel.DataAnnotations;

namespace MyModels
{
    public class StudySession 
    {
        public Guid Id { get; set; } = new Guid();

        public Guid UserId { get; set; }
        [Required]
        public User User { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; } 
        public DayOfWeek DayOfWeek { get; set; }

    }
}
