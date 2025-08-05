using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyModels;

namespace StudyPal.Controllers
{
    public class StudyController : Controller
    {
        private readonly ILogger<StudyController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public StudyController(ILogger<StudyController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public IActionResult Timetable()

        {
            return View();
        }
        public IActionResult Study()
        {
            return View();
        }
        public IActionResult SessionForm()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SessionForm(StudySession model)
        {
            if (ModelState.IsValid)
            {
                string userIdString = HttpContext.Session.GetString("UserId");

                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return RedirectToAction("Login");
                }
                DateTime today = DateTime.Today;
                var start = DateTime.Parse(today.ToShortDateString() + " " + model.StartTime.ToShortTimeString());
                var end = DateTime.Parse(today.ToShortDateString() + " " + model.EndTime.ToShortTimeString());

                model.StartTime = start;
                model.EndTime = end;
                model.UserId = userId;

                _unitOfWork.StudySession.Add(model);
               await  _unitOfWork.SaveAsync();

                TempData["success"] = "Study session started!";
                return RedirectToAction("Timer");
            }

            TempData["error"] = "Please correct the form";
            return View(model);
        }

        [HttpPost]
        [Route("api/study/save-session")]
        public async Task<IActionResult> SaveSession([FromBody] SessionDto dto)
        {
            Console.WriteLine("🔥 SaveSession endpoint hit!");
            if (ModelState.IsValid)
            {
                string userIdString = HttpContext.Session.GetString("UserId");

                if (!Guid.TryParse(userIdString, out Guid userId))
                    return RedirectToAction("Login");

                var session = new StudySession
                {
                    Id = Guid.NewGuid(),
                    Subject = dto.Subject,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Duration = dto.EndTime - dto.StartTime,
                    UserId = userId
                };

                _unitOfWork.StudySession.Add(session);
                await _unitOfWork.SaveAsync();
                Console.WriteLine("Session saved");
                return Ok("Session saved");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                 .Select(e => e.ErrorMessage)
                                 .ToList();
            return BadRequest(errors);
        }


    }
}
