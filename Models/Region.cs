using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegionCodeCollector.Models
{
    [Table("regions")]
    public class Region
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("admin_center")]
        [StringLength(100)]
        public string AdminCenter { get; set; } = string.Empty;

        [Column("interesting_fact")]
        public string? InterestingFact { get; set; }

        [Column("wiki_url")]
        [StringLength(255)]
        public string? WikiUrl { get; set; }

        public List<RegionCode> Codes { get; set; } = new();
    }
}