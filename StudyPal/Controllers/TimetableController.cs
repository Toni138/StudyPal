using DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using StudyPal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DataAccess.Repository.IRepository;
using MyModels;

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
    public IActionResult Create()
    {
        try
        {
            var model = new TimetableInputModel
            {
                Subjects = new List<Subject> { new Subject() },
                DailyStudyHours = Enum.GetValues(typeof(DayOfWeek))
                    .Cast<DayOfWeek>()
                    .Select(d => new DailyStudyHours { Day = d, Hours = 0 })
                    .ToList()
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading timetable create view");
            TempData["error"] = "Failed to load timetable creation page.";
            return View();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Generate(TimetableInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["error"] = "Please correct the form";
            return View("Create", model);
        }

        try
        {
            string userIdString = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                _logger.LogWarning("User not authenticated for timetable generation");
                TempData["error"] = "Please log in to generate a timetable.";
                return RedirectToAction("Login", "Account");
            }

            var timetable = await _generator.GenerateTimetableAsync(model.Subjects, model.DailyStudyHours, userId);
            TempData["success"] = "Timetable generated successfully!";
            return View("DisplayTimetable", timetable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating timetable");
            TempData["error"] = ex.Message;
            return View("Create", model);
        }
    }
}