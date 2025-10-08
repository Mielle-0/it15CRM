using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using AmazonReviewsCRM.Data;

namespace AmazonReviewsCRM.Controllers
{
    [Route("[controller]")]
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
                    Title = grp.Key.Length > 25 ? grp.Key.Substring(0, 25) + "..." : grp.Key,
                    FullTitle = grp.Key,
                    AvgRating = grp.Average(x => (double)x.ReviewScoreNumeric)
                })
                .OrderByDescending(x => x.AvgRating)
                .Take(5)
                .ToList();


            ViewBag.TopGameTitles = JsonSerializer.Serialize(topGames.Select(x => x.Title));
            ViewBag.TopGameFullTitles = JsonSerializer.Serialize(topGames.Select(x => x.FullTitle));
            ViewBag.TopGameRatings = JsonSerializer.Serialize(topGames.Select(x => x.AvgRating));


            // --- Alerts (Low Confidence Sentiment Reviews) ---
            var lowConfidenceAlerts = _context.ReviewSentiments
                .Where(rs => rs.ConfidenceScore < 0.5m)
                .OrderBy(rs => rs.ConfidenceScore)
                .Take(5)
                .Join(_context.Reviews,
                    rs => rs.ReviewId,
                    r => r.ReviewId,
                    (rs, r) => new { rs, r })
                .Join(_context.Games,
                    x => x.r.GameId,
                    g => g.GameId,
                    (x, g) => new
                    {
                        Game = g.AppName,
                        PredictedSentiment = x.rs.PredictedSentiment,
                        ConfidenceScore = x.rs.ConfidenceScore ?? 0m,
                        ReviewText = x.r.ReviewText.Length > 100 ? x.r.ReviewText.Substring(0, 100) + "..." : x.r.ReviewText,
                        ReviewDate = x.r.ReviewDate
                    })
                .ToList();

            ViewBag.Alerts = lowConfidenceAlerts;

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

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Dashboard controller is working!");
        }

        [HttpGet("export")]
        public IActionResult Export(string format = "csv")
        {
            try
            {
                Console.WriteLine($"[DEBUG] Export requested for format: {format}");
                
                // Get the same data as the dashboard
                var exportData = GetDashboardData();
                
                Console.WriteLine($"[DEBUG] Export data collected: {exportData.TotalReviews} reviews");
                
                switch (format.ToLower())
                {
                    case "csv":
                        Console.WriteLine("[DEBUG] Exporting as CSV");
                        return ExportToCsv(exportData);
                    case "excel":
                        Console.WriteLine("[DEBUG] Exporting as Excel");
                        return ExportToExcel(exportData);
                    case "json":
                        Console.WriteLine("[DEBUG] Exporting as JSON");
                        return ExportToJson(exportData);
                    case "pdf":
                        Console.WriteLine("[DEBUG] Exporting as PDF");
                        return ExportToPdf(exportData);
                    default:
                        Console.WriteLine($"[DEBUG] Unsupported format: {format}");
                        return BadRequest("Unsupported format");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Export failed: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Export failed: " + ex.Message);
            }
        }
        
        private DashboardExportData GetDashboardData()
        {
            // Collect all dashboard data
            var totalReviews = _context.Reviews.Count();
            
            var sentimentStats = _context.ReviewSentiments
                .GroupBy(s => s.PredictedSentiment.ToLower())
                .Select(g => new SentimentStatItem { Sentiment = g.Key, Count = g.Count() })
                .ToList();
                
            var lowConfidenceAlerts = _context.ReviewSentiments
                .Where(rs => rs.ConfidenceScore < 0.5m)
                .OrderBy(rs => rs.ConfidenceScore)
                .Take(10)
                .Join(_context.Reviews, rs => rs.ReviewId, r => r.ReviewId, (rs, r) => new { rs, r })
                .Join(_context.Games, x => x.r.GameId, g => g.GameId, (x, g) => new LowConfidenceAlert
                {
                    Game = g.AppName,
                    PredictedSentiment = x.rs.PredictedSentiment,
                    ConfidenceScore = x.rs.ConfidenceScore ?? 0m,
                    ReviewText = x.r.ReviewText,
                    ReviewDate = x.r.ReviewDate
                })
                .ToList();
                
            var helpfulReviews = _context.Reviews
                .Join(_context.Games, r => r.GameId, g => g.GameId, (r, g) => new HelpfulReviewItem
                {
                    Game = g.AppName,
                    Title = r.ReviewText,
                    Votes = r.ReviewVotes ?? 0
                })
                .OrderByDescending(x => x.Votes)
                .Take(5)
                .ToList();
                
            return new DashboardExportData
            {
                TotalReviews = totalReviews,
                SentimentStats = sentimentStats,
                LowConfidenceAlerts = lowConfidenceAlerts,
                HelpfulReviews = helpfulReviews,
                ExportDate = DateTime.Now
            };
        }
        
        private IActionResult ExportToCsv(DashboardExportData data)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Amazon Reviews CRM - Analytics Export");
            csv.AppendLine($"Export Date: {data.ExportDate:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine($"Total Reviews: {data.TotalReviews}");
            csv.AppendLine();
            
            // Sentiment Statistics
            csv.AppendLine("Sentiment Statistics");
            csv.AppendLine("Sentiment,Count");
            foreach (var stat in data.SentimentStats)
            {
                csv.AppendLine($"{stat.Sentiment},{stat.Count}");
            }
            csv.AppendLine();
            
            // Low Confidence Alerts
            csv.AppendLine("Low Confidence Alerts");
            csv.AppendLine("Game,Predicted Sentiment,Confidence Score,Review Date");
            foreach (var alert in data.LowConfidenceAlerts)
            {
                csv.AppendLine($"{alert.Game},{alert.PredictedSentiment},{alert.ConfidenceScore},{alert.ReviewDate:yyyy-MM-dd}");
            }
            
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"analytics_export_{DateTime.Now:yyyyMMdd}.csv");
        }
        
        private IActionResult ExportToJson(DashboardExportData data)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json = JsonSerializer.Serialize(data, jsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", $"analytics_export_{DateTime.Now:yyyyMMdd}.json");
        }
        
        private IActionResult ExportToExcel(DashboardExportData data)
        {
            // For now, return CSV with .xlsx extension (you can implement proper Excel export with EPPlus)
            var csv = new StringBuilder();
            csv.AppendLine("Amazon Reviews CRM - Analytics Export");
            csv.AppendLine($"Export Date\t{data.ExportDate:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine($"Total Reviews\t{data.TotalReviews}");
            csv.AppendLine();
            
            csv.AppendLine("Sentiment Statistics");
            csv.AppendLine("Sentiment\tCount");
            foreach (var stat in data.SentimentStats)
            {
                csv.AppendLine($"{stat.Sentiment}\t{stat.Count}");
            }
            
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "application/vnd.ms-excel", $"analytics_export_{DateTime.Now:yyyyMMdd}.xls");
        }
        
        private IActionResult ExportToPdf(DashboardExportData data)
        {
            // Simple text-based PDF (you can implement proper PDF generation with iTextSharp)
            var content = $@"Amazon Reviews CRM - Analytics Export
Export Date: {data.ExportDate:yyyy-MM-dd HH:mm:ss}
Total Reviews: {data.TotalReviews}

For full PDF support, please use CSV or Excel export.";
            var bytes = Encoding.UTF8.GetBytes(content);
            return File(bytes, "text/plain", $"analytics_export_{DateTime.Now:yyyyMMdd}.txt");
        }

        
    }
    
    public class DashboardExportData
    {
        public int TotalReviews { get; set; }
        public List<SentimentStatItem> SentimentStats { get; set; } = new();
        public List<LowConfidenceAlert> LowConfidenceAlerts { get; set; } = new();
        public List<HelpfulReviewItem> HelpfulReviews { get; set; } = new();
        public DateTime ExportDate { get; set; }
    }
    
    public class SentimentStatItem
    {
        public string Sentiment { get; set; } = string.Empty;
        public int Count { get; set; }
    }
    
    public class LowConfidenceAlert
    {
        public string Game { get; set; } = string.Empty;
        public string PredictedSentiment { get; set; } = string.Empty;
        public decimal ConfidenceScore { get; set; }
        public string ReviewText { get; set; } = string.Empty;
        public DateTime? ReviewDate { get; set; }
    }
    
    public class HelpfulReviewItem
    {
        public string Game { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Votes { get; set; }
    }
}
