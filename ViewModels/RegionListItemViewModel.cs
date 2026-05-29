namespace RegionCodeCollector.ViewModels
{
    public class RegionListItemViewModel
    {
        public string Code { get; set; } = string.Empty;

        public string RegionName { get; set; } = string.Empty;

        public string AdminCenter { get; set; } = string.Empty;

        public string? InterestingFact { get; set; }

        public string? WikiUrl { get; set; }

        public List<string> AllRegionCodes { get; set; } = new();
    }
}