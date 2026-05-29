namespace RegionCodeCollector.ViewModels
{
    public class ProgressViewModel
    {
        public int TotalRegions { get; set; }

        public int SeenRegions { get; set; }

        public int CompletedRegions { get; set; }

        public int TotalCodes { get; set; }

        public int SeenCodes { get; set; }

        public int RegionProgressPercent { get; set; }

        public int CodeProgressPercent { get; set; }
    }
}