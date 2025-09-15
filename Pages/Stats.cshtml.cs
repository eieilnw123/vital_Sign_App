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

            // กรองข้อมูลที่ RecordedAt มีค่าจริงๆ ออกมาก่อน
            var recordedDates = allData
                .Where(v => v.RecordedAt.HasValue)
                .Select(v => v.RecordedAt!.Value) // แปลงเป็น DateTime ปลอดภัยแล้ว
                .ToList();

            // นับจำนวนข้อมูลวันนี้
            TodayCount = recordedDates.Count(d => d.Date == today);

            // นับจำนวนข้อมูลสัปดาห์นี้
            ThisWeekCount = recordedDates.Count(d => d.Date >= startOfWeek);

            // นับจำนวนข้อมูลเดือนนี้
            ThisMonthCount = recordedDates.Count(d => d.Date >= startOfMonth);

            // Group ข้อมูลรายชั่วโมง (เฉพาะข้อมูลวันนี้)
            HourlyData = recordedDates
                .Where(d => d.Date == today)
                .GroupBy(d => d.Hour)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => $"{g.Key:00}:00-{g.Key:00}:59", // เช่น 09:00-09:59
                    g => g.Count()
                );
        }


    }
}