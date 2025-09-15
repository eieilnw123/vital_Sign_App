using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using VitalSignApp.Hubs;
using VitalSignApp.Models;
using VitalSignApp.Services;

namespace VitalSignApp.Pages
{
    [IgnoreAntiforgeryToken]
    public class VitalSignsModel : PageModel
    {
        private readonly VitalSignApiService _vitalSignApiService;
        private readonly IHubContext<VitalSignHub> _hubContext;
        private readonly IConfiguration _configuration;

        public List<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();

        // สำหรับ JS (เชื่อม SignalR)
        public string HubUrl { get; set; } = string.Empty;

        // เพิ่ม Properties สำหรับ Enhanced Pagination
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1; // หน้าปัจจุบัน (เริ่มต้นที่หน้า 1)

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 12; // จำนวนรายการต่อหน้า (รองรับการเปลี่ยนแปลง)

        public int TotalCount { get; set; } // จำนวนรายการทั้งหมด
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize); // จำนวนหน้าทั้งหมด

        // สำหรับแสดงข้อมูลสถิติ
        public int StartItem => (CurrentPage - 1) * PageSize + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalCount);

        public VitalSignsModel(
            VitalSignApiService vitalSignApiService,
            IHubContext<VitalSignHub> hubContext,
            IConfiguration configuration)
        {
            _vitalSignApiService = vitalSignApiService;
            _hubContext = hubContext;
            _configuration = configuration;

            // โหลดค่า HubUrl จาก appsettings.json
            HubUrl = _configuration["HubSettings:HubUrl"] ?? string.Empty;
        }

        public async Task OnGetAsync()
        {
            // ตรวจสอบและ validate PageSize
            if (PageSize <= 0 || PageSize > 100)
            {
                PageSize = 12; // Default value
            }

            // ตรวจสอบ CurrentPage
            if (CurrentPage <= 0)
            {
                CurrentPage = 1;
            }

            // ดึงข้อมูลทั้งหมดจาก API
            var allVitalSigns = await _vitalSignApiService.GetVitalSignsAsync();

            // คำนวณ Pagination
            TotalCount = allVitalSigns.Count;

            // ตรวจสอบว่า CurrentPage ไม่เกิน TotalPages
            if (CurrentPage > TotalPages && TotalPages > 0)
            {
                CurrentPage = TotalPages;
            }

            VitalSigns = allVitalSigns
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostAddVitalSignAsync(VitalSign newVitalSign)
        {
            if (await _vitalSignApiService.AddVitalSignAsync(newVitalSign))
            {
                await _hubContext.Clients.All.SendAsync("ReceiveVitalSignUpdate", newVitalSign);
                return new JsonResult(new { success = true });
            }
            return new JsonResult(new { success = false });
        }

        public async Task<IActionResult> OnPostUpdateVitalSignAsync(VitalSign updatedVitalSign)
        {
            // ตรวจสอบ ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest("Model binding failed in Razor Page");
            }

            // เรียก Service -> PUT API
            bool success = await _vitalSignApiService.UpdateVitalSignAsync(updatedVitalSign);
            return new JsonResult(new { success });
        }

        public async Task<IActionResult> OnGetSearchAsync(string? hn)
        {
            // ตรวจสอบและ validate PageSize สำหรับการค้นหา
            if (PageSize <= 0 || PageSize > 100)
            {
                PageSize = 12;
            }

            if (CurrentPage <= 0)
            {
                CurrentPage = 1;
            }

            // ดึงข้อมูลตาม HN ที่ค้นหา
            var allVitalSigns = await _vitalSignApiService.GetVitalSignsByHnAsync(hn);

            TotalCount = allVitalSigns.Count;

            // ตรวจสอบว่า CurrentPage ไม่เกิน TotalPages
            if (CurrentPage > TotalPages && TotalPages > 0)
            {
                CurrentPage = TotalPages;
            }

            VitalSigns = allVitalSigns
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }

        // เพิ่ม Method สำหรับจัดการ PageSize แบบ AJAX (Optional)
        public async Task<IActionResult> OnGetChangePageSizeAsync(int pageSize, string? hn = null)
        {
            PageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 12;
            CurrentPage = 1; // Reset to first page when changing page size

            if (!string.IsNullOrEmpty(hn))
            {
                return await OnGetSearchAsync(hn);
            }
            else
            {
                await OnGetAsync();
                return Page();
            }
        }
    }
}