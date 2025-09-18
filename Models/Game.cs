using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AmazonReviewsCRM.Models
{
    [Table("Games")]
    public class Game
    {
        [Column("game_id")]
        public int GameId { get; set; }
        [Column("app_id")]
        public string? AppId { get; set; }

        [Column("app_name")]
        public string AppName { get; set; } = null!;

        [Column("genre")]
        public string? Genre { get; set; }

        [Column("release_date")]
        public DateTime? ReleaseDate { get; set; }

        [Column("developer")]
        public string? Developer { get; set; }

        [Column("publisher")]
        public string? Publisher { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("source")]
        public string? Source { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }



    [Table("ArchivedGames")]
    public class ArchivedGame
    {
        [Key]
        [Column("archived_game_id")]
        public int ArchivedGameId { get; set; }

        [Column("original_game_id")]
        public int OriginalGameId { get; set; }

        [Column("app_id")]
        public string? AppId { get; set; }

        [Column("app_name")]
        public string AppName { get; set; } = null!;

        [Column("genre")]
        public string? Genre { get; set; }

        [Column("release_date")]
        public DateTime? ReleaseDate { get; set; }

        [Column("developer")]
        public string? Developer { get; set; }

        [Column("publisher")]
        public string? Publisher { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("source")]
        public string? Source { get; set; }

        [Column("archived_at")]
        public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
    }

}
