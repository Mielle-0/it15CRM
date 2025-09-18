using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using AmazonReviewsCRM.Data;

namespace AmazonReviewsCRM.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            // --- KPI cards ---
            var totalReviews = _context.Reviews.Count();

            // sentiments (normalize lowercase)
            var sentimentStats = _context.ReviewSentiments
                .GroupBy(s => s.PredictedSentiment.ToLower())
                .Select(g => new { Sentiment = g.Key, Count = g.Count() })
                .ToList();

            int positive = sentimentStats.FirstOrDefault(s => s.Sentiment == "positive")?.Count ?? 0;
            int negative = sentimentStats.FirstOrDefault(s => s.Sentiment == "negative")?.Count ?? 0;

            double positivePct = totalReviews > 0 ? (positive * 100.0 / totalReviews) : 0;
            double negativePct = totalReviews > 0 ? (negative * 100.0 / totalReviews) : 0;

            // average rating (handle "5.0")
            // average rating (directly from decimal column)
            double avgRating = _context.Reviews
                .Select(r => (double?)r.ReviewScoreNumeric)
                .Average() ?? 0;


            // push to ViewBag
            ViewBag.TotalReviews = totalReviews;
            ViewBag.PositivePct = positivePct;
            ViewBag.NegativePct = negativePct;
            ViewBag.AvgRating = avgRating;


            // --- Sentiment distribution (reuse sentimentStats) ---
            var sentimentDict = sentimentStats
                .GroupBy(s => s.Sentiment ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

            ViewBag.SentimentLabels = JsonSerializer.Serialize(sentimentDict.Keys);
            ViewBag.SentimentValues = JsonSerializer.Serialize(sentimentDict.Values);

            // --- Top 5 games (only numeric scores like Amazon "5.0") ---
            var topGames = _context.Reviews
                .Join(_context.Games,
                    r => r.GameId,
                    g => g.GameId,
                    (r, g) => new { g.AppName, r.ReviewScoreNumeric })
                .Where(x => x.ReviewScoreNumeric != null)
                .GroupBy(x => x.AppName)
                .Select(grp => new {
                    Title = grp.Key,
                    AvgRating = grp.Average(x => (double)x.ReviewScoreNumeric)
                })
                .OrderByDescending(x => x.AvgRating)
                .Take(5)
                .ToList();


            ViewBag.TopGameTitles = JsonSerializer.Serialize(topGames.Select(x => x.Title));
            ViewBag.TopGameRatings = JsonSerializer.Serialize(topGames.Select(x => x.AvgRating));


            // --- Alerts ---
            var cutoff = DateTime.UtcNow.AddDays(-30);

            var alerts = _context.Reviews
                .Join(_context.Games,
                    r => r.GameId,
                    g => g.GameId,
                    (r, g) => new { r, g })
                .Where(x => x.r.ReviewScoreNumeric != null)
                .GroupBy(x => new { x.g.AppName })
                .Select(grp => new
                {
                    Game = grp.Key.AppName,
                    PrevAvg = grp.Where(x => x.r.ReviewDate < cutoff)
                                .Average(x => (decimal?)x.r.ReviewScoreNumeric) ?? 0m,
                    RecentAvg = grp.Where(x => x.r.ReviewDate >= cutoff)
                                .Average(x => (decimal?)x.r.ReviewScoreNumeric) ?? 0m
                })
                .Where(x => x.PrevAvg > 0 && x.RecentAvg > 0 && Math.Abs(x.PrevAvg - x.RecentAvg) >= 1.0m)
                .ToList();


            ViewBag.Alerts = alerts;

            // --- Most helpful reviews ---
            ViewBag.Helpful = _context.Reviews
                .Join(_context.Games,
                    r => r.GameId,
                    g => g.GameId,
                      (r, g) => new
                      {
                          Game = g.AppName,
                          Title = r.ReviewText,
                          Votes = r.ReviewVotes ?? 0
                      })
                .OrderByDescending(x => x.Votes)
                .Take(5)
                .ToList();

            return View();
        }

        
    }
}
