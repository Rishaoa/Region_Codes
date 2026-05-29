namespace RegionCodeCollector.ViewModels
{
    public class RegionCodeStatusViewModel
    {
        public int RegionCodeId { get; set; }

        public string Code { get; set; } = string.Empty;

        public bool IsSeen { get; set; }

        public DateTime? SeenAt { get; set; }

        public string? Note { get; set; }
    }
}