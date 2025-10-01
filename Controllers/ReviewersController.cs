using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonReviewsCRM.Controllers
{
    // [Authorize]
    public class ReviewersController : Controller
    {
        // GET: /Reviewers
        public IActionResult Index()
        {
            // Hardcoded mock reviewers
            var reviewers = new List<dynamic>
            {
                new { Id = 1, Name = "Alice Johnson", TotalVotes = 3200, PositiveReviews = 58, NeutralReviews = 12, NegativeReviews = 8, Email = "alice@example.com" },
                new { Id = 2, Name = "Bob Smith", TotalVotes = 2100, PositiveReviews = 40, NeutralReviews = 15, NegativeReviews = 20, Email = "bob@example.com" },
                new { Id = 3, Name = "Charlie Nguyen", TotalVotes = 1500, PositiveReviews = 22, NeutralReviews = 18, NegativeReviews = 12, Email = "charlie@example.com" },
                new { Id = 4, Name = "Dana Lee", TotalVotes = 900, PositiveReviews = 15, NeutralReviews = 6, NegativeReviews = 9, Email = "dana@example.com" },
            };

            return View(reviewers);
        }
    }
}
