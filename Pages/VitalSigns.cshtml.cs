using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
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

        public List<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();

        // เพิ่ม Properties สำหรับ Pagination
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1; // หน้าปัจจุบัน (เริ่มต้นที่หน้า 1)
        public int PageSize { get; set; } = 12; // จำนวนรายการต่อหน้า
        public int TotalCount { get; set; } // จำนวนรายการทั้งหมด
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize); // จำนวนหน้าทั้งหมด

        public VitalSignsModel(VitalSignApiService vitalSignApiService, IHubContext<VitalSignHub> hubContext)
        {
            _vitalSignApiService = vitalSignApiService;
            _hubContext = hubContext;
        }

        public async Task OnGetAsync()
        {
            // ดึงข้อมูลทั้งหมดจาก API
            var allVitalSigns = await _vitalSignApiService.GetVitalSignsAsync();

            // คำนวณ Pagination
            TotalCount = allVitalSigns.Count;
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

    }
}
