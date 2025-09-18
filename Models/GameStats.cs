using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AmazonReviewsCRM.Models
{
    [Keyless]
    [Table("GameStats")]
    public class GameStats
    {
        [Column("app_id")]
        public int ReviewCount { get; set; }
        public double? AvgSentiment { get; set; }
        public double RecommendedPct { get; set; }
        public double NotRecommendedPct { get; set; }
    }

}