using System.ComponentModel.DataAnnotations.Schema;

namespace RegionCodeCollector.Models
{
    [Table("user_seen_codes")]
    public class UserSeenCode
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        [Column("region_code_id")]
        public int RegionCodeId { get; set; }

        public RegionCode RegionCode { get; set; } = null!;

        [Column("seen_at")]
        public DateTime SeenAt { get; set; }

        [Column("note")]
        public string? Note { get; set; }
    }
}