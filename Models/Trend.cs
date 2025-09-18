using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AmazonReviewsCRM.Models
{
    public class SentimentTrendsViewModel
    {
        public List<string> Labels { get; set; } = new();
        public List<GameSentiment> Games { get; set; } = new();
        public List<AnomalyViewModel> Anomalies { get; set; } = new();
    }

    public class GameSentiment
    {
        public string Name { get; set; } = string.Empty;
        public List<double> Sentiments { get; set; } = new();
    }

    public class AnomalyViewModel
    {
        public string Game { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Note { get; set; } = string.Empty;
    }


    [Keyless]
    public class ViewSentimentTrend
    {
        [Column("game_id")]
        public int GameId { get; set; }

        [Column("app_name")]
        public string AppName { get; set; } = string.Empty;

        [Column("year_month")]
        public string YearMonth { get; set; } = string.Empty;

        [Column("avg_sentiment")]
        public double AvgSentiment { get; set; }

        [Column("review_count")]
        public int ReviewCount { get; set; }
    }

}