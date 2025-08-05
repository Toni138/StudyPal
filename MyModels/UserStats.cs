using MyModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class UserStats
{
    public Guid Id { get; set; }            
    public Guid UserId { get; set; }

    public DateTime? LastActivityDate { get; set; }
    public int LongestStreak { get; set; }
    public int CurrentStreak { get; set; }
    public int TotalStudyHours { get; set; }
    public double AverageDailyStudyHours { get; set; }
    public List<SubjectStudyHours> HoursPerSubject { get; set; } = new List<SubjectStudyHours>();
    public int FlashcardsReviewed { get; set; }

    public User User { get; set; }
}
