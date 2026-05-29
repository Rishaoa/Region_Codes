using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RegionCodeCollector.Data;
using RegionCodeCollector.ViewModels;

namespace RegionCodeCollector.Pages.Regions
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<RegionListItemViewModel> Regions { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        public async Task OnGetAsync()
        {
            var codesFromDb = await _context.RegionCodes
                .AsNoTracking()
                .Include(regionCode => regionCode.Region)
                .ToListAsync();

            var allCodesByRegionId = codesFromDb
                .GroupBy(regionCode => regionCode.RegionId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(regionCode =>
                        {
                            bool isNumber = int.TryParse(regionCode.Code, out int codeNumber);

                            return new
                            {
                                Code = regionCode.Code,
                                IsNumber = isNumber,
                                CodeNumber = codeNumber
                            };
                        })
                        .Where(code => code.IsNumber)
                        .OrderBy(code => code.CodeNumber)
                        .Select(code => code.Code)
                        .ToList()
                );

            var rows = codesFromDb
                .Select(regionCode =>
                {
                    bool isNumber = int.TryParse(regionCode.Code, out int codeNumber);

                    return new
                    {
                        RegionCode = regionCode,
                        IsNumber = isNumber,
                        CodeNumber = codeNumber
                    };
                })
                .Where(item => item.IsNumber)
                .Where(item => item.CodeNumber >= 1 && item.CodeNumber <= 99)
                .OrderBy(item => item.CodeNumber)
                .Select(item => new RegionListItemViewModel
                {
                    Code = item.CodeNumber.ToString("D2"),
                    RegionName = item.RegionCode.Region.Name,
                    AdminCenter = item.RegionCode.Region.AdminCenter,
                    InterestingFact = item.RegionCode.Region.InterestingFact,
                    WikiUrl = item.RegionCode.Region.WikiUrl,
                    AllRegionCodes = allCodesByRegionId[item.RegionCode.RegionId]
                })
                .ToList();

            Regions = ApplySearch(rows);
        }

        private List<RegionListItemViewModel> ApplySearch(List<RegionListItemViewModel> rows)
        {
            IEnumerable<RegionListItemViewModel> query = rows;

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string search = SearchQuery.Trim().ToLower();

                query = query.Where(row =>
                    row.Code.Contains(search) ||
                    row.RegionName.ToLower().Contains(search) ||
                    row.AdminCenter.ToLower().Contains(search) ||
                    row.AllRegionCodes.Any(code => code.Contains(search))
                );
            }

            return query
                .OrderBy(row => int.Parse(row.Code))
                .ToList();
        }
    }
}