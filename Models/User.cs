using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegionCodeCollector.Models
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("username")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("password_hash")]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public List<UserSeenCode> SeenCodes { get; set; } = new();
    }
}