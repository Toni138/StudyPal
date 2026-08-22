using DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using StudyPal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DataAccess.Repository.IRepository;
using MyModels;
using Microsoft.AspNetCore.Http;

namespace StudyPal.Controllers;

public class TimetableController : Controller
{
    private readonly ILogger<TimetableController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimetableGenerator _generator;

    public TimetableController(ILogger<TimetableController> logger, IUnitOfWork unitOfWork, TimetableGenerator generator)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _generator = generator;
    }

    [HttpGet]
    public IActionResult DisplayTimetable()
    {
        return View();
    }

    [HttpGet]
    [HttpGet]
    public IActionResult Create()
    {
        try
        {
            var model = new TimetableInputModel
            {
                Subjects = new List<Subject> { new Subject() },
                DailyFreeSlots = Enum.GetValues(typeof(DayOfWeek))
                    .Cast<DayOfWeek>()
                    .Select(day => new DailyFreeSlots
                    {
                        Day = day,
                        FreeSlots = new List<TimeSlot>
                        {
                        new TimeSlot
                        {
                            StartTime = new TimeOnly(0, 0),   // Changed to 00:00
                            EndTime   = new TimeOnly(0, 0)    // Changed to 00:00
                        }
                        }
                    })
                    .ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading timetable create view");
            TempData["error"] = "Failed to load timetable creation page.";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(TimetableInputModel model)
    {
        // Custom validation that allows 00:00-00:00 as "No free time"
        ValidateTimeSlots(model);

        if (!ModelState.IsValid)
        {
            // Reinitialize missing collections for redisplay
            if (model.Subjects == null || !model.Subjects.Any())
                model.Subjects = new List<Subject> { new Subject() };

            if (model.DailyFreeSlots == null || !model.DailyFreeSlots.Any())
            {
                model.DailyFreeSlots = Enum.GetValues(typeof(DayOfWeek))
                    .Cast<DayOfWeek>()
                    .Select(d => new DailyFreeSlots
                    {
                        Day = d,
                        FreeSlots = new List<TimeSlot> { new TimeSlot() }
                    })
                    .ToList();
            }

            return View("Create", model);   // No TempData needed if you show ModelState errors
        }

        // === Successful case ===
        try
        {
            string userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                TempData["error"] = "Please log in to generate a timetable.";
                return RedirectToAction("Login", "Account");
            }

            var timetable = await _generator.GenerateTimetableAsync(
                model.Subjects,
                model.DailyFreeSlots,
                userId);

            TempData["success"] = "Timetable generated successfully!";
            return View("DisplayTimetable", timetable);   
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating timetable");
            TempData["error"] = "An error occurred while generating the timetable.";
            return View("Create", model);
        }
    }

    // Updated Validation - Allows 00:00-00:00 as valid "No free time"
    private void ValidateTimeSlots(TimetableInputModel model)
    {
        if (model.DailyFreeSlots == null) return;

        for (int dayIndex = 0; dayIndex < model.DailyFreeSlots.Count; dayIndex++)
        {
            var day = model.DailyFreeSlots[dayIndex];
            if (day.FreeSlots == null || !day.FreeSlots.Any())
            {
                ModelState.AddModelError($"DailyFreeSlots[{dayIndex}].FreeSlots",
                    $"Please add at least one time slot for {day.Day}.");
                continue;
            }

            for (int slotIndex = 0; slotIndex < day.FreeSlots.Count; slotIndex++)
            {
                var slot = day.FreeSlots[slotIndex];

                // Special case: 00:00 - 00:00 means "No free time this day" → allowed
                bool isNoFreeTime = slot.StartTime == new TimeOnly(0, 0) &&
                                    slot.EndTime == new TimeOnly(0, 0);

                if (!isNoFreeTime)
                {
                    if (slot.EndTime <= slot.StartTime)
                    {
                        ModelState.AddModelError(
                            $"DailyFreeSlots[{dayIndex}].FreeSlots[{slotIndex}].EndTime",
                            "End time must be after start time."
                        );
                    }

                    int duration = slot.DurationMinutes;   // Assuming you have this property

                    if (duration < 30)
                    {
                        ModelState.AddModelError(
                            $"DailyFreeSlots[{dayIndex}].FreeSlots[{slotIndex}].EndTime",
                            "Study slot must be at least 30 minutes long."
                        );
                    }

                    if (duration > 480) // 8 hours
                    {
                        ModelState.AddModelError(
                            $"DailyFreeSlots[{dayIndex}].FreeSlots[{slotIndex}].EndTime",
                            "A single study slot cannot exceed 8 hours."
                        );
                    }
                }
                // If it IS 00:00-00:00, we silently accept it (no error)
            }
        }
    }
}
