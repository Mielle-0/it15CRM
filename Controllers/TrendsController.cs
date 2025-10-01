using Microsoft.AspNetCore.Mvc;
using AmazonReviewsCRM.Data;
using AmazonReviewsCRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AmazonReviewsCRM.Security;

namespace AmazonReviewsCRM.Controllers
{
    // [Authorize]
    public class TrendsController : Controller
    {
        private readonly AppDbContext _context;

        public TrendsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Trends
        // [RequireRoleAccess("Trends")]
        public async Task<IActionResult> Index(
            string? searchApp,
            string? startDate,
            string? endDate,
            double? minSentiment,
            int? minReviews)
        {

            var query = _context.ViewSentimentTrends.AsNoTracking();

            // Default: last 3 months + current
            var defaultStart = DateTime.UtcNow.AddMonths(-3);
            var defaultEnd = DateTime.UtcNow;

            // Parse date range (YearMonth is string "yyyy-MM")
            var start = !string.IsNullOrEmpty(startDate)
                ? DateTime.Parse(startDate + "-01")
                : defaultStart;
            var end = !string.IsNullOrEmpty(endDate)
                ? DateTime.Parse(endDate + "-01").AddMonths(1).AddDays(-1) // last day of month
                : defaultEnd;

            // Apply filters
            if (!string.IsNullOrEmpty(searchApp))
            {
                query = query.Where(t => t.AppName.Contains(searchApp));
            }

            query = query.Where(t =>
                string.Compare(t.YearMonth, start.ToString("yyyy-MM")) >= 0 &&
                string.Compare(t.YearMonth, end.ToString("yyyy-MM")) <= 0);

            if (minSentiment.HasValue)
            {
                query = query.Where(t => t.AvgSentiment >= minSentiment.Value);
            }

            if (minReviews.HasValue && minReviews.Value > 0)
            {
                query = query.Where(t => t.ReviewCount >= minReviews.Value);
            }

            var trends = await query
                .OrderBy(t => t.YearMonth)
                .ToListAsync();

            if (!trends.Any())
            {
                return View(new SentimentTrendsViewModel());
            }

            // Build labels (months)
            var labels = trends
                .Select(t => t.YearMonth)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            // Group by game
            var games = trends
                .GroupBy(t => t.AppName)
                .Select(g => new GameSentiment
                {
                    Name = g.Key,
                    Sentiments = labels.Select(month =>
                        g.FirstOrDefault(x => x.YearMonth == month)?.AvgSentiment ?? 0
                    ).ToList()
                })
                .ToList();

            // Detect anomalies
            var anomalies = new List<AnomalyViewModel>();
            foreach (var game in games)
            {
                for (int i = 1; i < game.Sentiments.Count; i++)
                {
                    var prev = game.Sentiments[i - 1];
                    var curr = game.Sentiments[i];
                    if (Math.Abs(curr - prev) >= 0.3)
                    {
                        anomalies.Add(new AnomalyViewModel
                        {
                            Game = game.Name,
                            Month = labels[i],
                            Value = curr,
                            Note = curr > prev ? "Spike detected" : "Drop detected"
                        });
                    }
                }
            }

            var viewModel = new SentimentTrendsViewModel
            {
                Labels = labels,
                Games = games,
                Anomalies = anomalies
            };

            return View(viewModel);
        }




    }
}
