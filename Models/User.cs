using System.ComponentModel.DataAnnotations.Schema;

namespace AmazonReviewsCRM.Models
{
    [Table("User")] 
    public class User
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("role")]
        public string? Role { get; set; }

        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        [Column("active")]
        public bool Active { get; set; }
    }

}