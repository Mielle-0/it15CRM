using Microsoft.EntityFrameworkCore;
using AmazonReviewsCRM.Models;

namespace AmazonReviewsCRM.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Game> Games { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewSentiment> ReviewSentiments { get; set; }
        public DbSet<ViewGameList> ViewGameList { get; set; }
        public DbSet<ReviewOverview> ReviewOverview { get; set; }
        public DbSet<ArchivedGame> ArchivedGames { get; set; }
        public DbSet<ArchivedReview> ArchivedReviews { get; set; }
        public DbSet<ViewSentimentTrend> ViewSentimentTrends { get; set; }
        public DbSet<User> Users { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Table mappings
            modelBuilder.Entity<Game>().ToTable("Games");
            modelBuilder.Entity<Review>().ToTable("Reviews");
            modelBuilder.Entity<ReviewSentiment>().ToTable("ReviewSentiment");
            modelBuilder.Entity<ArchivedGame>().ToTable("ArchivedGames");
            modelBuilder.Entity<ArchivedReview>().ToTable("ArchivedReviews");
            modelBuilder.Entity<User>().ToTable("Users");



            // Define primary keys explicitly
            modelBuilder.Entity<Game>().HasKey(g => g.GameId);
            modelBuilder.Entity<Review>().HasKey(r => r.ReviewId);
            modelBuilder.Entity<ReviewSentiment>().HasKey(s => s.SentimentId);
            modelBuilder.Entity<ArchivedGame>().HasKey(a => a.ArchivedGameId);
            modelBuilder.Entity<ArchivedReview>().HasKey(a => a.ArchivedReviewId);
            modelBuilder.Entity<User>().HasKey(u => u.UserId);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Game)
                .WithMany(g => g.Reviews)
                .HasForeignKey(r => r.GameId);

            modelBuilder.Entity<ReviewSentiment>()
                .HasOne(s => s.Review)
                .WithMany(r => r.Sentiments)
                .HasForeignKey(s => s.ReviewId);

            modelBuilder.Entity<ReviewOverview>()
                .HasNoKey()
                .ToView("viewReviewOverview");

            modelBuilder.Entity<ViewGameList>()
                .HasNoKey()
                .ToView("viewGameList");

            modelBuilder.Entity<ViewSentimentTrend>()
                .HasNoKey()
                .ToView("viewSentimentTrends");

        }
    }
}