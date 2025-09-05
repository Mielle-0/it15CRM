using Microsoft.AspNetCore.Mvc;

namespace AmazonReviewsCRM.Controllers
{
    public class GamesController : Controller
    {
        // ✅ Declare the hardcoded game list as a class field
        private readonly List<dynamic> _games = new()
        {
            new { AppId = 1001, AppName = "Space Explorer", Genre="Action", AvgSentiment = 0.82, RecommendedPct = 90, NotRecommendedPct = 10, ReviewCount = 1200, ReleaseDate = new DateTime(2023, 1, 15) },
            new { AppId = 1002, AppName = "Farm Tycoon", Genre="Simulation", AvgSentiment = 0.65, RecommendedPct = 70, NotRecommendedPct = 30, ReviewCount = 850, ReleaseDate = new DateTime(2022, 5, 2) },
            new { AppId = 1003, AppName = "Dungeon Quest", Genre="RPG", AvgSentiment = 0.45, RecommendedPct = 40, NotRecommendedPct = 60, ReviewCount = 400, ReleaseDate = new DateTime(2021, 11, 20) },
            new { AppId = 1004, AppName = "City Builder Pro", Genre="Strategy", AvgSentiment = 0.78, RecommendedPct = 85, NotRecommendedPct = 15, ReviewCount = 2100, ReleaseDate = new DateTime(2024, 3, 10) },
            new { AppId = 1005, AppName = "Racing Xtreme", Genre="Racing", AvgSentiment = 0.72, RecommendedPct = 75, NotRecommendedPct = 25, ReviewCount = 1300, ReleaseDate = new DateTime(2023, 7, 18) },
        };

        // GET: /Games
        public IActionResult Index()
        {
            return View(_games);
        }

        // GET: /Games/Details/{id}
        public IActionResult Details(int id)
        {
            var game = _games.FirstOrDefault(g => g.AppId == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }
    }
}
