using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmazonReviewsCRM.Models
{
    [Table("Reviews")]
    public class Review
    {
        [Column("review_id")]
        public int ReviewId { get; set; }

        [Column("review_text")]
        public string? ReviewText { get; set; }

        [Column("review_score_numeric")]
        public decimal? ReviewScoreNumeric { get; set; }

        [Column("review_recommendation")]
        public bool? ReviewRecommendation { get; set; }


        [Column("review_votes")]
        public int? ReviewVotes { get; set; }

        [Column("review_date")]
        public DateTime? ReviewDate { get; set; }

        [Column("reviewer_id")]
        public string? ReviewerId { get; set; }
        
        [Column("game_id")]
        public int GameId { get; set; }  
        public Game Game { get; set; } = null!;



        public ICollection<ReviewSentiment> Sentiments { get; set; } = new List<ReviewSentiment>();
    }


    public class ReviewOverview
    {
        public int? ReviewId { get; set; }
        public string? AppName { get; set; }
        public string? ReviewText { get; set; }
        public string? Sentiment { get; set; }
        public decimal? SentimentConfidence { get; set; }
        public string? Source { get; set; }
        public DateTime? ReviewDate { get; set; }
    }

    public class ReviewDetailViewModel
    {
        // Game Info
        public string AppName { get; set; } = null!;
        public string? Genre { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public string? Source { get; set; }

        // Review Info
        public int ReviewId { get; set; }
        public string? ReviewText { get; set; }
        public decimal? ReviewScoreNumeric { get; set; }
        public bool? ReviewRecommendation { get; set; }
        public int? ReviewVotes { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? ReviewerId { get; set; }

        // Sentiment Info
        public string? PredictedSentiment { get; set; }
        public decimal? ConfidenceScore { get; set; }
    }


    [Table("ArchivedReviews")]
    public class ArchivedReview
    {
        [Key]
        [Column("archived_review_id")]
        public int ArchivedReviewId { get; set; }

        [Column("original_review_id")]
        public int OriginalReviewId { get; set; }

        [Column("review_text")]
        public string? ReviewText { get; set; } = null!;

        [Column("review_votes")]
        public int? ReviewVotes { get; set; }

        [Column("review_date")]
        public DateTime? ReviewDate { get; set; }

        [Column("reviewer_id")]
        public string ReviewerId { get; set; } = null!;

        [Column("review_score_numeric")]
        public decimal? ReviewScoreNumeric { get; set; }

        [Column("review_recommendation")]
        public bool? ReviewRecommendation { get; set; }

        [Column("original_game_id")]
        public int OriginalGameId { get; set; }

        [Column("archived_at")]
        public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
    }




}
