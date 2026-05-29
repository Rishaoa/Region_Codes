using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RegionCodeCollector.Data;
using RegionCodeCollector.Models;
using RegionCodeCollector.ViewModels;

namespace RegionCodeCollector.Pages.MyRegions
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<MyRegionViewModel> Regions { get; set; } = new();

        public ProgressViewModel Progress { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "all";

        private class PageData
        {
            public List<MyRegionViewModel> Regions { get; set; } = new();

            public ProgressViewModel Progress { get; set; } = new();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Redirect("/Account/Login");
            }

            PageData pageData = await BuildPageDataAsync(userId.Value);

            Regions = pageData.Regions;
            Progress = pageData.Progress;

            return Page();
        }

        public async Task<IActionResult> OnPostToggleCodeAsync(int regionCodeId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Redirect("/Account/Login");
            }

            await ToggleCodeAsync(userId.Value, regionCodeId);

            return Redirect(BuildReturnUrl());
        }

        public async Task<IActionResult> OnPostSaveNoteAsync(int regionCodeId, string? note)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Redirect("/Account/Login");
            }

            await SaveNoteAsync(userId.Value, regionCodeId, note);

            return Redirect(BuildReturnUrl());
        }

        public async Task<IActionResult> OnPostToggleCodeAjaxAsync(int regionCodeId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return new UnauthorizedResult();
            }

            var regionCode = await _context.RegionCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(code => code.Id == regionCodeId);

            if (regionCode == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Код не найден."
                });
            }

            await ToggleCodeAsync(userId.Value, regionCodeId);

            PageData pageData = await BuildPageDataAsync(userId.Value);

            var regionRow = pageData.Regions
                .FirstOrDefault(row => row.RegionId == regionCode.RegionId);

            if (regionRow == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Регион не найден."
                });
            }

            return new JsonResult(new
            {
                success = true,
                progress = BuildProgressJson(pageData.Progress),
                region = BuildRegionJson(regionRow),
                codes = BuildCodesJson(regionRow)
            });
        }

        public async Task<IActionResult> OnPostSaveNoteAjaxAsync(int regionCodeId, string? note)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return new UnauthorizedResult();
            }

            await SaveNoteAsync(userId.Value, regionCodeId, note);

            string? savedNote = string.IsNullOrWhiteSpace(note)
                ? null
                : note.Trim();

            return new JsonResult(new
            {
                success = true,
                regionCodeId,
                note = savedNote
            });
        }

        private async Task ToggleCodeAsync(int userId, int regionCodeId)
        {
            var existingSeenCode = await _context.UserSeenCodes
                .FirstOrDefaultAsync(seenCode =>
                    seenCode.UserId == userId &&
                    seenCode.RegionCodeId == regionCodeId);

            if (existingSeenCode == null)
            {
                var newSeenCode = new UserSeenCode
                {
                    UserId = userId,
                    RegionCodeId = regionCodeId,
                    SeenAt = DateTime.Now
                };

                _context.UserSeenCodes.Add(newSeenCode);
            }
            else
            {
                _context.UserSeenCodes.Remove(existingSeenCode);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SaveNoteAsync(int userId, int regionCodeId, string? note)
        {
            var seenCode = await _context.UserSeenCodes
                .FirstOrDefaultAsync(item =>
                    item.UserId == userId &&
                    item.RegionCodeId == regionCodeId);

            if (seenCode != null)
            {
                seenCode.Note = string.IsNullOrWhiteSpace(note)
                    ? null
                    : note.Trim();

                await _context.SaveChangesAsync();
            }
        }

        private string BuildReturnUrl()
        {
            var queryParts = new List<string>();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                queryParts.Add($"SearchQuery={Uri.EscapeDataString(SearchQuery)}");
            }

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                queryParts.Add($"StatusFilter={Uri.EscapeDataString(StatusFilter)}");
            }

            if (queryParts.Count == 0)
            {
                return "/MyRegions";
            }

            return "/MyRegions?" + string.Join("&", queryParts);
        }

        private async Task<PageData> BuildPageDataAsync(int userId)
        {
            var codesFromDb = await _context.RegionCodes
                .AsNoTracking()
                .Include(regionCode => regionCode.Region)
                .ToListAsync();

            var userSeenCodes = await _context.UserSeenCodes
                .AsNoTracking()
                .Where(seenCode => seenCode.UserId == userId)
                .ToListAsync();

            var seenCodesByRegionCodeId = userSeenCodes
                .ToDictionary(seenCode => seenCode.RegionCodeId);

            var codesByRegionId = codesFromDb
                .GroupBy(regionCode => regionCode.RegionId)
                .ToDictionary(
                    group => group.Key,
                    group => group
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
                        .Where(code => code.IsNumber)
                        .OrderBy(code => code.CodeNumber)
                        .Select(code =>
                        {
                            bool isSeen = seenCodesByRegionCodeId.TryGetValue(
                                code.RegionCode.Id,
                                out UserSeenCode? seenCode
                            );

                            return new RegionCodeStatusViewModel
                            {
                                RegionCodeId = code.RegionCode.Id,
                                Code = code.RegionCode.Code,
                                IsSeen = isSeen,
                                SeenAt = seenCode?.SeenAt,
                                Note = seenCode?.Note
                            };
                        })
                        .ToList()
                );

            ProgressViewModel progress = BuildProgress(codesByRegionId);

            var allRows = codesFromDb
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
                .Select(item =>
                {
                    List<RegionCodeStatusViewModel> regionCodes =
                        codesByRegionId[item.RegionCode.RegionId];

                    int totalCodesCount = regionCodes.Count;
                    int seenCodesCount = regionCodes.Count(code => code.IsSeen);

                    string status;
                    string statusText;

                    if (seenCodesCount == 0)
                    {
                        status = "not-found";
                        statusText = "Не собран";
                    }
                    else if (seenCodesCount < totalCodesCount)
                    {
                        status = "partial";
                        statusText = "Частично собран";
                    }
                    else
                    {
                        status = "completed";
                        statusText = "Полностью собран";
                    }

                    int collectionPercent = totalCodesCount == 0
                        ? 0
                        : (int)Math.Round((double)seenCodesCount / totalCodesCount * 100);

                    return new MyRegionViewModel
                    {
                        RegionId = item.RegionCode.RegionId,
                        CurrentRegionCodeId = item.RegionCode.Id,
                        Code = item.CodeNumber.ToString("D2"),
                        RegionName = item.RegionCode.Region.Name,
                        WikiUrl = item.RegionCode.Region.WikiUrl,
                        TotalCodesCount = totalCodesCount,
                        SeenCodesCount = seenCodesCount,
                        CollectionPercent = collectionPercent,
                        Status = status,
                        StatusText = statusText,
                        AllRegionCodes = regionCodes.Select(code => code.Code).ToList(),
                        Codes = regionCodes
                    };
                })
                .ToList();

            return new PageData
            {
                Regions = ApplyFilters(allRows),
                Progress = progress
            };
        }

        private ProgressViewModel BuildProgress(
            Dictionary<int, List<RegionCodeStatusViewModel>> codesByRegionId)
        {
            int totalRegions = codesByRegionId.Count;

            int seenRegions = codesByRegionId.Count(region =>
                region.Value.Any(code => code.IsSeen));

            int completedRegions = codesByRegionId.Count(region =>
                region.Value.Count > 0 &&
                region.Value.All(code => code.IsSeen));

            int totalCodes = codesByRegionId.Sum(region =>
                region.Value.Count);

            int seenCodes = codesByRegionId.Sum(region =>
                region.Value.Count(code => code.IsSeen));

            return new ProgressViewModel
            {
                TotalRegions = totalRegions,
                SeenRegions = seenRegions,
                CompletedRegions = completedRegions,
                TotalCodes = totalCodes,
                SeenCodes = seenCodes,
                RegionProgressPercent = totalRegions == 0
                    ? 0
                    : (int)Math.Round((double)seenRegions / totalRegions * 100),
                CodeProgressPercent = totalCodes == 0
                    ? 0
                    : (int)Math.Round((double)seenCodes / totalCodes * 100)
            };
        }

        private List<MyRegionViewModel> ApplyFilters(List<MyRegionViewModel> allRows)
        {
            IEnumerable<MyRegionViewModel> query = allRows;

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string search = SearchQuery.Trim().ToLower();

                query = query.Where(row =>
                    row.Code.Contains(search) ||
                    row.RegionName.ToLower().Contains(search) ||
                    row.AllRegionCodes.Any(code => code.Contains(search))
                );
            }

            if (!string.IsNullOrWhiteSpace(StatusFilter) && StatusFilter != "all")
            {
                query = query.Where(row => row.Status == StatusFilter);
            }

            return query
                .OrderBy(row => int.Parse(row.Code))
                .ToList();
        }

        private object BuildProgressJson(ProgressViewModel progress)
        {
            int completedProgressPercent = progress.TotalRegions == 0
                ? 0
                : (int)Math.Round((double)progress.CompletedRegions / progress.TotalRegions * 100);

            return new
            {
                totalRegions = progress.TotalRegions,
                seenRegions = progress.SeenRegions,
                completedRegions = progress.CompletedRegions,
                totalCodes = progress.TotalCodes,
                seenCodes = progress.SeenCodes,
                regionProgressPercent = progress.RegionProgressPercent,
                codeProgressPercent = progress.CodeProgressPercent,
                completedProgressPercent
            };
        }

        private object BuildRegionJson(MyRegionViewModel region)
        {
            return new
            {
                regionId = region.RegionId,
                status = region.Status,
                statusText = region.StatusText,
                seenCodesCount = region.SeenCodesCount,
                totalCodesCount = region.TotalCodesCount,
                collectionPercent = region.CollectionPercent
            };
        }

        private object BuildCodesJson(MyRegionViewModel region)
        {
            return region.Codes.Select(code => new
            {
                regionCodeId = code.RegionCodeId,
                isSeen = code.IsSeen,
                seenAt = code.SeenAt?.ToString("dd.MM.yyyy HH:mm"),
                note = code.Note
            });
        }
    }
}