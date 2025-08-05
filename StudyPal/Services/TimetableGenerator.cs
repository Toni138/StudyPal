using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using MyModels;
using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Repository.IRepository;

namespace StudyPal.Services;

public class TimetableGenerator
{
    private readonly IUnitOfWork _unitOfWork;

    public TimetableGenerator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<StudySession>> GenerateTimetableAsync(List<Subject> subjects, List<DailyStudyHours> dailyStudyHours, Guid userId)
    {
        var timetable = new List<StudySession>();
        var random = new Random();

        foreach (var day in dailyStudyHours.Where(d => d.Hours > 0))
        {
            int remainingHours = day.Hours;
            var availableSubjects = subjects.OrderBy(s => random.Next()).ToList();

            DateTime startTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 0, 0); // Start at 9 AM
            foreach (var subject in availableSubjects)
            {
                if (remainingHours <= 0) break;

                int sessionHours = Math.Min(remainingHours, GetHoursForDifficulty(subject.Difficulty));
                var session = new StudySession
                {
                    UserId = userId, // Use the passed userId
                    Subject = subject.Name,
                    StartTime = startTime,
                    EndTime = startTime.AddHours(sessionHours),
                    DayOfWeek = day.Day
                };

                timetable.Add(session);
                _unitOfWork.StudySession.Add(session);
                remainingHours -= sessionHours;
                startTime = startTime.AddHours(sessionHours + 0.5); // 30-min break
            }
        }

        await _unitOfWork.SaveAsync();
        return timetable;
    }

    private int GetHoursForDifficulty(string difficulty)
    {
        return difficulty switch
        {
            "Easy" => 1,
            "Medium" => 2,
            "Hard" => 3,
            _ => 1
        };
    }
}