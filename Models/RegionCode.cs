using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegionCodeCollector.Models
{
    [Table("region_codes")]
    public class RegionCode
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("region_id")]
        public int RegionId { get; set; }

        public Region Region { get; set; } = null!;

        [Required]
        [Column("code")]
        [StringLength(3)]
        public string Code { get; set; } = string.Empty;

        public List<UserSeenCode> UserSeenCodes { get; set; } = new();
    }
}