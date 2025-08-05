using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace StudyPal.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public AccountController(ILogger<AccountController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            string userIdString = HttpContext.Session.GetString("UserId");

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToAction("Login", "Home");
            }
            var user = _unitOfWork.User.Get(f => f.Id == userId);
            return View(user);
        }
    }
}
