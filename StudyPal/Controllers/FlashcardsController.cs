using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using MyModels;
using Utility;

namespace StudyPal.Controllers
{
    public class FlashcardsController:Controller
    {
            private readonly ILogger<FlashcardsController> _logger;
            private readonly ApplicationDbContext _dbcontext;
            private readonly IUnitOfWork _unitOfWork;
            public FlashcardsController(ILogger<FlashcardsController> logger, ApplicationDbContext dbcontext, IUnitOfWork unitOfWork)
            {
                _logger = logger;
                _dbcontext = dbcontext;
                _unitOfWork = unitOfWork;
            }
        public IActionResult Index()
        {
            string userIdString = HttpContext.Session.GetString("UserId");

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToAction("Login", "Home");
            }
            var userFlashcards = _unitOfWork.Flashcard.GetAll(f => f.UserId == userId).OrderBy(f => f.NextReviewTime).ToList();

            return View(userFlashcards);
        }


        public IActionResult Create()
        {
            string userIdString = HttpContext.Session.GetString("UserId");

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToAction("Login", "Home");
            }

            var tags = _unitOfWork.Flashcard
                .GetAll(f => f.UserId == userId)
              .Where(f => f.Tag != null)
              .Select(f => f.Tag.ToLower())
                .Distinct()
                .ToList();

            ViewBag.TagList = tags;

            return View();
        }

        public IActionResult Edit(Guid id)
        {
            string userIdString = HttpContext.Session.GetString("UserId");

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToAction("Login", "Home");
            }

            var flashcard = _unitOfWork.Flashcard.Get(
                f => f.Id == id && f.UserId == userId
            );

            if (flashcard == null)
            {
                return NotFound(); 
            }

            return View(flashcard);
        }

        public IActionResult Review(Guid id)
        {
            string userIdString = HttpContext.Session.GetString("UserId");

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToAction("Login", "Home");
            }

            var flashcard = _unitOfWork.Flashcard.Get(
                f => f.Id == id && f.UserId == userId
            );

            if (flashcard == null)
            {
                return NotFound();
            }

            return View(flashcard);
        }
        public IActionResult FlashcardsByTag(string tagName)
        {
            string userIdString = HttpContext.Session.GetString("UserId");

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToAction("Login", "Home");
            }

            var userFlashcards = _unitOfWork.Flashcard
                .GetAll(f => f.UserId == userId &&
                             (string.IsNullOrEmpty(tagName) || f.Tag.ToLower().Contains(tagName.ToLower())))
                .OrderBy(f => f.NextReviewTime)
                .ToList();
            ViewData["TagName"] = tagName;
            return View("Index", userFlashcards);
        }

        [HttpPost]
        
        public IActionResult Search(string searchTerm)
        {
            string userIdString = HttpContext.Session.GetString("UserId");

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToAction("Login", "Home");
            }

            var userFlashcards = _unitOfWork.Flashcard
                .GetAll(f => f.UserId == userId &&
                             (string.IsNullOrEmpty(searchTerm) || f.Question.ToLower().Contains(searchTerm.ToLower())))
                .OrderBy(f => f.NextReviewTime)
                .ToList();
            ViewData["SearchTerm"] = searchTerm;
            return View("Index", userFlashcards);
        }
        [HttpPost]
        public IActionResult ClearSessionAndReturn()
        {
            HttpContext.Session.Remove("ReviewSession");
            HttpContext.Session.Remove("ReviewedCards");

            return RedirectToAction("Index", "Flashcards");
        }
        [HttpPost]
        public async Task<IActionResult> Create(Flashcard obj)
        {
            if (obj.Tag.Contains(','))
            {
                ModelState.AddModelError("Tag", "Tags can't contain commas");
            }

            string userIdString = HttpContext.Session.GetString("UserId");

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToAction("Login", "Home");
            }

            // Check if the tag already exists for this user
            //bool tagExists = _unitOfWork.Flashcard
            //    .GetAll(f => f.UserId == userId)
            // .Any(f => f.Tag != null && f.Tag.Equals(obj.Tag, StringComparison.OrdinalIgnoreCase));


            //if (tagExists)
            //{
            //    ModelState.AddModelError("Tag", "This tag already exists.");
            //}

            if (ModelState.IsValid)
            {
                obj.UserId = userId;
                obj.CreatedAt = DateTime.Now;
                obj.NextReviewTime = DateTime.Now;

                _unitOfWork.Flashcard.Add(obj);
                await _unitOfWork.SaveAsync();

                TempData["success"] = "Flashcard created successfully";
                return RedirectToAction("Index");
            }

            ViewBag.TagList = _unitOfWork.Flashcard
                .GetAll(f => f.UserId == userId).Where(f => f.Tag != null)
                .Select(f => f.Tag.ToLower())
                .Distinct()
                .ToList();

            return View(obj);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Flashcard obj)
        {

            if (obj.Tag.Contains(','))
            {
                ModelState.AddModelError(obj.Tag, "Tags can't contain commas");
            }

            if (ModelState.IsValid)
            {
                obj.NextReviewTime = DateTime.Now;

                _unitOfWork.Flashcard.Update(obj);
                await _unitOfWork.SaveAsync();

                TempData["success"] = "Flashcard edited successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var flashcard = _unitOfWork.Flashcard.Get(f => f.Id == id);
            if (flashcard == null)
            {
                return NotFound();
            }

            _unitOfWork.Flashcard.Remove(flashcard);
            await _unitOfWork.SaveAsync();

            TempData["success"] = "Flashcard deleted successfully!";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> MarkReview(Guid id, bool isCorrect)
        {
            var flashcard = _unitOfWork.Flashcard.Get(f => f.Id == id);
            if (flashcard == null) return NotFound();

            // Update the flashcard's next review time
            flashcard.NextReviewTime = isCorrect ? DateTime.Now.AddDays(3) : DateTime.Now.AddDays(1);

            // Update user stats
            var UserStats = _unitOfWork.UserStats.Get(f => f.UserId == flashcard.UserId);
            UserStats.FlashcardsReviewed += 1;
            bool hasReviewedFlashcardsToday = true;
            var streakManager = new StreakManager(_unitOfWork);
            streakManager.UpdateStreak(UserStats,hasReviewedFlashcardsToday,null);

            // Save the changes
            _unitOfWork.Flashcard.Update(flashcard);
            await _unitOfWork.SaveAsync();

            // Check if we're in a structured review session
            var reviewSessionCards = HttpContext.Session.GetString("ReviewSession");

            if (!string.IsNullOrEmpty(reviewSessionCards))
            {
                // We're in a review session - continue to next card
                var sessionCardIds = reviewSessionCards.Split(',').Select(Guid.Parse).ToList();

                // Get already reviewed cards
                var reviewedCards = HttpContext.Session.GetString("ReviewedCards");
                var reviewedCardIds = new List<Guid>();

                if (!string.IsNullOrEmpty(reviewedCards))
                {
                    reviewedCardIds = reviewedCards.Split(',').Select(Guid.Parse).ToList();
                }

                // Add current card to reviewed list
                reviewedCardIds.Add(id);
                HttpContext.Session.SetString("ReviewedCards", string.Join(",", reviewedCardIds));

                // Find next card in session
                var nextCardId = sessionCardIds.FirstOrDefault(cardId => !reviewedCardIds.Contains(cardId));

                if (nextCardId != Guid.Empty)
                {
                    var remainingCards = sessionCardIds.Count - reviewedCardIds.Count;
                    TempData["success"] = $"Review recorded! {remainingCards} cards remaining.";
                    return RedirectToAction("Review", new { id = nextCardId });
                }
                else
                {
                    // Session completed
                    HttpContext.Session.Remove("ReviewSession");
                    HttpContext.Session.Remove("ReviewedCards");
                    TempData["success"] = "🎉 Review session completed! Great job!";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                // Individual card review - just go back to index
                TempData["success"] = "Review recorded!";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public IActionResult StartReview(int count)
        {
            // Get user ID from session
            string userIdString = HttpContext.Session.GetString("UserId");

            if (!Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login");


            // Validate count
            if (count <= 0)
            {
                TempData["error"] = "Please enter a valid number of flashcards to review.";
                return RedirectToAction("Index");
            }

            // Get user's flashcards
            var userFlashcards = _unitOfWork.Flashcard
                .GetAll(f => f.UserId == userId)
                .OrderBy(f => f.NextReviewTime) // Prioritize cards due for review
                .Take(count) // Take only the requested number
                .ToList();

            // Check if user has any flashcards
            if (!userFlashcards.Any())
            {
                TempData["error"] = "You don't have any flashcards to review. Create some first!";
                return RedirectToAction("Index");
            }

            // Check if user requested more cards than available
            if (userFlashcards.Count < count)
            {
                TempData["info"] = $"You only have {userFlashcards.Count} flashcards. Starting review with all available cards.";
            }

            // Clear any existing review session
            HttpContext.Session.Remove("ReviewedCards");

            // Set up the review session with the selected cards
            var reviewCardIds = userFlashcards.Select(f => f.Id).ToList();
            HttpContext.Session.SetString("ReviewSession", string.Join(",", reviewCardIds));

            // Start with the first card
            var firstCard = userFlashcards.First();

            TempData["success"] = $"Review session started! {userFlashcards.Count} cards to review.";
            return RedirectToAction("Review", new { id = firstCard.Id });
        }


    }
}
