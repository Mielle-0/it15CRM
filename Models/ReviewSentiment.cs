using System.ComponentModel.DataAnnotations.Schema;

namespace AmazonReviewsCRM.Models
{
    [Table("ReviewSentiment")]
    public class ReviewSentiment
    {
        [Column("sentiment_id")]
        public int SentimentId { get; set; }

        [Column("review_id")]
        public int ReviewId { get; set; }

        [Column("predicted_sentiment")]
        public string? PredictedSentiment { get; set; }

        [Column("confidence_score")]
        public decimal? ConfidenceScore { get; set; }

        public Review Review { get; set; } = null!;
    }
}
