using Microsoft.AspNetCore.Mvc;

namespace AmazonReviewsCRM.Controllers
{
    public class TrendsController : Controller
    {
        // GET: /Trends
        public IActionResult Index()
        {
            // Mock sentiment data for multiple games
            var sentimentData = new
            {
                Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul" },
                Games = new[]
                {
                    new { Name = "Space Explorer", Sentiments = new[] { 0.75, 0.80, 0.78, 0.82, 0.70, 0.65, 0.72 } },
                    new { Name = "Farm Tycoon", Sentiments = new[] { 0.60, 0.58, 0.65, 0.68, 0.70, 0.66, 0.63 } },
                    new { Name = "Dungeon Quest", Sentiments = new[] { 0.45, 0.50, 0.55, 0.48, 0.40, 0.42, 0.44 } }
                },
                Anomalies = new[]
                {
                    new { Game = "Space Explorer", Month = "Jun", Value = 0.65, Note = "Post-launch bug reports" },
                    new { Game = "Dungeon Quest", Month = "Mar", Value = 0.55, Note = "Big patch boosted reviews" }
                }
            };

            return View(sentimentData);
        }
    }
}
