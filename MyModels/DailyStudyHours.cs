
using System.ComponentModel.DataAnnotations;

public class DailyStudyHours
{
    public DayOfWeek Day { get; set; }

    [Range(0, 24, ErrorMessage = "Hours must be between 0 and 24")]
    public int Hours { get; set; }
}