using DataAccess.Repository.IRepository;
using MyModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudyPal.Services;

public class TimetableGenerator
{
    private readonly IUnitOfWork _unitOfWork;

    public TimetableGenerator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<StudySession>> GenerateTimetableAsync(
        List<Subject> subjects,
        List<DailyFreeSlots> dailyFreeSlots,
        Guid userId)
    {
        // Remove invalid subjects
        subjects = subjects.Where(s => !string.IsNullOrWhiteSpace(s.Name)).ToList();

        if (subjects.Count == 0)
            throw new ArgumentException("At least one valid subject is required");

        int totalAvailableMinutes = CalculateTotalAvailableMinutes(dailyFreeSlots);

        if (totalAvailableMinutes < 10)
            throw new ArgumentException("Not enough free time to generate a meaningful timetable.");

        // STEP 1: Assign weights
        var weightedSubjects = subjects.Select(s => new
        {
            Subject = s,
            Weight = GetDifficultyWeight(s.Difficulty)
        }).ToList();

        int totalWeight = weightedSubjects.Sum(ws => ws.Weight);

        // STEP 2: Estimate sessions and breaks
        int estimatedSessions = (int)Math.Ceiling((double)totalAvailableMinutes / 60);
        int estimatedBreaks = Math.Max(0, estimatedSessions - 1);
        int totalBreakTime = estimatedBreaks * 10;

        // STEP 3: Calculate study time after reserving breaks
        int availableForStudy = totalAvailableMinutes - totalBreakTime;

        if (availableForStudy <= 0)
            throw new ArgumentException("Not enough time after accounting for breaks");

        // STEP 4: Ensure each subject gets at least 10 mins
        int minRequired = subjects.Count * 10;

        if (availableForStudy < minRequired)
            throw new ArgumentException("Not enough time to include all subjects with minimum duration");

        availableForStudy -= minRequired;

        // STEP 5: Distribute remaining time proportionally
        var budgets = weightedSubjects.ToDictionary(
            ws => ws.Subject.Name,
            ws => 10 + (int)Math.Round((double)availableForStudy * ws.Weight / totalWeight)
        );

        var timetable = new List<StudySession>();

        // STEP 6: Schedule sessions
        foreach (var day in dailyFreeSlots)
        {
            if (day.FreeSlots == null || !day.FreeSlots.Any()) continue;

            bool hasSessionToday = false;

            foreach (var slot in day.FreeSlots.Where(s => s.EndTime > s.StartTime))
            {
                DateTime currentTime = DateTime.Today
                    .AddDays((int)day.Day - (int)DateTime.Today.DayOfWeek)
                    .Add(slot.StartTime.ToTimeSpan());

                int remainingInSlot = (int)(slot.EndTime - slot.StartTime).TotalMinutes;

                while (remainingInSlot > 0)
                {
                    var next = budgets
                        .Where(b => b.Value > 0)
                        .OrderByDescending(b => b.Value)
                        .FirstOrDefault();

                    if (next.Key == null) goto SlotDone;

                    // Add break if not first session
                    if (hasSessionToday)
                    {
                        if (remainingInSlot < 10) break;

                        currentTime = currentTime.AddMinutes(10);
                        remainingInSlot -= 10;
                    }

                    // Session must be between 10 and 60 mins
                    int sessionMinutes = Math.Min(60, Math.Min(next.Value, remainingInSlot));

                    if (sessionMinutes < 10) break;

                    var session = new StudySession
                    {
                        UserId = userId,
                        Subject = next.Key,
                        StartTime = currentTime,
                        EndTime = currentTime.AddMinutes(sessionMinutes),
                        DayOfWeek = day.Day
                    };

                    timetable.Add(session);
                    _unitOfWork.StudySession.Add(session);

                    budgets[next.Key] -= sessionMinutes;

                    currentTime = currentTime.AddMinutes(sessionMinutes);
                    remainingInSlot -= sessionMinutes;

                    hasSessionToday = true;
                }
            }

        SlotDone:;
        }

        await _unitOfWork.SaveAsync();
        return timetable;
    }

    private int CalculateTotalAvailableMinutes(List<DailyFreeSlots> dailyFreeSlots)
    {
        int total = 0;

        foreach (var day in dailyFreeSlots)
        {
            if (day.FreeSlots == null) continue;

            foreach (var slot in day.FreeSlots)
            {
                if (slot.EndTime > slot.StartTime)
                    total += (int)(slot.EndTime - slot.StartTime).TotalMinutes;
            }
        }

        return total;
    }

    private int GetDifficultyWeight(string difficulty)
    {
        return difficulty?.ToLower() switch
        {
            "hard" => 3,
            "medium" => 2,
            "easy" => 1,
            _ => 1
        };
    }
}