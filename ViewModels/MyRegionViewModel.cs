namespace RegionCodeCollector.ViewModels
{
    public class MyRegionViewModel
    {
        public int RegionId { get; set; }
        public int CurrentRegionCodeId { get; set; }

        public string Code { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string? WikiUrl { get; set; }

        public int TotalCodesCount { get; set; }
        public int SeenCodesCount { get; set; }

        public int CollectionPercent { get; set; }

        public string Status { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;

        public List<string> AllRegionCodes { get; set; } = new();
        public List<RegionCodeStatusViewModel> Codes { get; set; } = new();
    }
}