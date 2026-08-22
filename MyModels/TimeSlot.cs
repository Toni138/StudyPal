using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyModels
{
    public class TimeSlot
    {
        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        public TimeOnly StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        public TimeOnly EndTime { get; set; }

        // Custom validation to ensure EndTime > StartTime
        public bool IsValid => EndTime > StartTime;

        // Helper property for duration
        public int DurationMinutes => EndTime > StartTime
            ? (int)(EndTime - StartTime).TotalMinutes
            : 0;
    }
}
