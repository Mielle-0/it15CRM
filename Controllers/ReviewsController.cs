using Microsoft.AspNetCore.Mvc;

namespace AmazonReviewsCRM.Controllers
{
    public class ReviewsController : Controller
    {
        // GET: /Reviews
        public IActionResult Index()
        {
            // Hardcoded mock reviews
            var reviews = new List<dynamic>
            {
                new { Id = 1, Reviewer = "Alice", Content = "Amazing game, very immersive!", Sentiment = "Positive", Helpfulness = 120, Date = new DateTime(2025, 8, 1), Flagged = false },
                new { Id = 2, Reviewer = "Bob", Content = "Too many bugs ruined the fun.", Sentiment = "Negative", Helpfulness = 45, Date = new DateTime(2025, 7, 22), Flagged = false },
                new { Id = 3, Reviewer = "Charlie", Content = "Decent but needs improvement in combat.", Sentiment = "Neutral", Helpfulness = 67, Date = new DateTime(2025, 6, 18), Flagged = true },
            };

            return View(reviews);
        }
    }
}
