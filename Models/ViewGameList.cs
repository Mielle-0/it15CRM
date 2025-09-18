using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AmazonReviewsCRM.Models
{
    [Keyless]
    public class ViewGameList
    {
        [Column("game_id")]
        public int GameId { get; set; }
        [Column("app_id")]
        public string? AppId { get; set; }
        public string? AppName { get; set; }
        public string? Genre { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public string? Source { get; set; }

        public int ReviewCount { get; set; }
        public decimal AvgSentiment { get; set; }  // -1.0 (negative) → +1.0 (positive)
    }
    public class PagedResult<T>
    {
        public IEnumerable<T>? Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}