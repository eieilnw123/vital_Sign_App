using Microsoft.AspNetCore.Mvc.RazorPages;
using VitalSignApp.Services;
using VitalSignApp.Models;
using System.Globalization;

namespace VitalSignApp.Pages
{
    public class StatsModel : PageModel
    {
        private readonly VitalSignApiService _apiService;

        public int TodayCount { get; set; }
        public int ThisWeekCount { get; set; }
        public int ThisMonthCount { get; set; }
        public Dictionary<string, int> HourlyData { get; set; } = new();

        public StatsModel(VitalSignApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task OnGetAsync()
        {
            var allData = await _apiService.GetVitalSignsAsync();

            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            TodayCount = allData.Count(v => v.RecordedAt.Date == today);
            ThisWeekCount = allData.Count(v => v.RecordedAt >= startOfWeek);
            ThisMonthCount = allData.Count(v => v.RecordedAt >= startOfMonth);

            // Group by hour for today
            HourlyData = allData
                .Where(v => v.RecordedAt.Date == today)
                .GroupBy(v => v.RecordedAt.Hour)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => $"{g.Key:00}:00-{g.Key:00}:59",
                    g => g.Count()
                );
        }
    }
}