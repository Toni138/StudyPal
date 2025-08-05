using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyModels;
using DataAccess;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Utility;
using System.Threading.Tasks;

namespace StudyPal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        public IActionResult ConfirmEmail(Guid id)
        {
            var user = _unitOfWork.User.Get(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);  // Pass the user to the view so you can access the ID
        }
        public IActionResult Dashboard()
        {
            string username = HttpContext.Session.GetString("Username");
            string userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                // Handle invalid or missing userId here, e.g. redirect to login
                return RedirectToAction("Login");
            }

            var userStats = _unitOfWork.UserStats.Get(us => us.UserId == userId);

            return View(userStats);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _unitOfWork.User.Get(u => u.EmailAddress == model.EmailAddress);
            if (user != null)
            {
                ModelState.AddModelError(string.Empty, "The email is already registered. Login or create a new account");
                return View(model);
            }

            user = new User
            {
                Id = Guid.NewGuid(),
                Username = model.Username,
                EmailAddress = model.EmailAddress,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            var userStats = new UserStats
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LongestStreak = 0,
                CurrentStreak = 0,
                TotalStudyHours = 0,
                AverageDailyStudyHours = 0,
                HoursPerSubject = new List<SubjectStudyHours>()
            };
            user.UserStats = userStats;

            _unitOfWork.User.Add(user);
            _unitOfWork.UserStats.Add(userStats); 
           await _unitOfWork.SaveAsync();

            TempData["success"] = "Registration Successful.";
            return RedirectToAction("ConfirmEmail", new { id = user.Id });
        }

        public async Task<IActionResult> ConfirmEmailPost(Guid id)
        {
            var user = _unitOfWork.User.Get(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsEmailVerified = true;
            _unitOfWork.User.Update(user);
           await  _unitOfWork.SaveAsync();

            TempData["success"] = "Email confirmed successfully!";
            return RedirectToAction("Login");
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            //I should put something here so that if a particular user has more than 3 failed attempts something should be done
            //var failedLoginAttempts = 0;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _unitOfWork.User.Get(u => u.EmailAddress == model.UsernameorEmail);
            if (user == null) {
                user = _unitOfWork.User.Get(u => u.Username == model.UsernameorEmail);
            }
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Account is not registered");
                //failedLoginAttempts += 1;
                return View(model);
            }
            bool passwordIsValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
            if (!passwordIsValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                //failedLoginAttempts += 1;
                return View(model);
            }

            if (!user.IsEmailVerified)
            {
                ModelState.AddModelError(string.Empty, "Please confirm your email before logging in.");
                return RedirectToAction("ConfirmEmail", new { id = user.Id });

            }
            //user.FailedLoginAttempts = failedLoginAttempts;
           await  _unitOfWork.SaveAsync();
            HttpContext.Session.SetString("AppRestartToken", AppSessionValidator.AppRestartToken);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            return RedirectToAction("Dashboard");
        }


        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            
            return RedirectToAction("Index");
        }




        }
}
