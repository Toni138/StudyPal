using System;

namespace MyModels
{
    public class TimetableStudySessionViewModel
    {
        public string Subject { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public int DurationHours { get; set; }
        public int SessionNumber { get; set; }

        public string DurationDisplay => $"{DurationHours} hour{(DurationHours > 1 ? "s" : "")}";
    }
}