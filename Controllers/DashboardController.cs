using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AmazonReviewsCRM.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // --- Hardcoded demo data ---
            var reviews = new List<dynamic>
            {
                new { Game="Game A", Rating=5, Sentiment="Positive", Votes=120, Date=DateTime.UtcNow.AddDays(-5), Title="Amazing!" },
                new { Game="Game A", Rating=4, Sentiment="Positive", Votes=80, Date=DateTime.UtcNow.AddDays(-15), Title="Good game" },
                new { Game="Game B", Rating=2, Sentiment="Negative", Votes=10, Date=DateTime.UtcNow.AddDays(-3), Title="Buggy" },
                new { Game="Game C", Rating=3, Sentiment="Neutral", Votes=25, Date=DateTime.UtcNow.AddDays(-20), Title="It’s okay" },
                new { Game="Game D", Rating=5, Sentiment="Positive", Votes=200, Date=DateTime.UtcNow.AddDays(-7), Title="Masterpiece" },
                new { Game="Game E", Rating=1, Sentiment="Negative", Votes=5, Date=DateTime.UtcNow.AddDays(-2), Title="Terrible" },
                new { Game="Game F", Rating=5, Sentiment="Positive", Votes=150, Date=DateTime.UtcNow.AddDays(-30), Title="Love it" },
                new { Game="Game B", Rating=1, Sentiment="Negative", Votes=50, Date=DateTime.UtcNow.AddDays(-10), Title="Not fun" }
            };

            // Total reviews
            ViewBag.TotalReviews = reviews.Count;

            // Sentiment distribution
            var sentimentDict = reviews
                .GroupBy(r => r.Sentiment)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.SentimentLabels = JsonSerializer.Serialize(sentimentDict.Keys);
            ViewBag.SentimentValues = JsonSerializer.Serialize(sentimentDict.Values);

            // Top 5 games
            var topGames = reviews
                .GroupBy(r => r.Game)
                .Select(g => new {
                    Title = g.Key,
                    AvgRating = g.Average(r => (int)r.Rating)
                })
                .OrderByDescending(x => x.AvgRating)
                .Take(5)
                .ToList();

            ViewBag.TopGameTitles = JsonSerializer.Serialize(topGames.Select(x => x.Title));
            ViewBag.TopGameRatings = JsonSerializer.Serialize(topGames.Select(x => x.AvgRating));

            // Alerts
            ViewBag.Alerts = new List<dynamic>
            {
                new { Game="Game B", PrevAvg=3.5, RecentAvg=2.0 },
                new { Game="Game E", PrevAvg=2.5, RecentAvg=1.0 }
            };

            // Most helpful reviews
            ViewBag.Helpful = reviews
                .OrderByDescending(r => r.Votes)
                .Take(5)
                .ToList();

            return View();
        }
    }
}
