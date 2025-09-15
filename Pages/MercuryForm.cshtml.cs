using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitalSignApp.Models;
using VitalSignApp.Services;
using System.Text.Json;

namespace VitalSignApp.Pages
{
    public class MercuryFormModel : PageModel
    {
        private readonly VitalSignApiService _apiService;

        // รับค่า HN ที่ผู้ใช้ค้นหาจาก URL (Query String)
        [BindProperty(SupportsGet = true)]
        public string? SearchHn { get; set; }

        // เก็บข้อมูล Vital Signs ที่ได้จาก API
        public List<VitalSign> VitalSignsData { get; set; } = new();

        // เก็บข้อมูลในรูปแบบ JSON เพื่อส่งให้ JavaScript
        public string ChartDataJson { get; set; } = "[]";

        public MercuryFormModel(VitalSignApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task OnGetAsync()
        {
            // ตรวจสอบว่ามีการค้นหา HN หรือไม่
            if (!string.IsNullOrEmpty(SearchHn))
            {
                // เรียกใช้ Service เพื่อดึงข้อมูลตาม HN
                VitalSignsData = await _apiService.GetVitalSignsByHnAsync(SearchHn);

                // เตรียมข้อมูลสำหรับ Chart.js
                // โดยแปลง List<VitalSign> ให้อยู่ในรูปแบบที่ JavaScript ใช้งานง่าย
                var chartData = VitalSignsData
                    .Where(v => v.RecordedAt.HasValue)
                    .OrderBy(v => v.RecordedAt)
                    .Select(v => new
                    {
                        time = v.RecordedAt!.Value.ToString("yyyy-MM-dd HH:mm"), // Format ที่ JavaScript จัดการง่าย
                        pulse = v.Pulse,
                        temp = v.Temp,
                        respiration = v.RespiratoryRate,
                        systolic = v.Systolic,
                        diastolic = v.Diastolic,
                        spo2 = v.Sp02
                    });

                // แปลงข้อมูลเป็น JSON String เพื่อส่งไปหน้าเว็บ
                ChartDataJson = JsonSerializer.Serialize(chartData);
            }
        }
    }
}