using System.Net.Http;
using System.Net.Http.Json;
using VitalSignApp.Models;

namespace VitalSignApp.Services
{
    public class VitalSignApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public VitalSignApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // เมธอดดึงข้อมูล Vital Signs จาก API
        public async Task<List<VitalSign>> GetVitalSignsAsync()
        {

            var client = _httpClientFactory.CreateClient("VitalSignApi");
            return await client.GetFromJsonAsync<List<VitalSign>>("VitalSign") ?? new List<VitalSign>();
        }

        // เมธอดเพิ่มข้อมูล Vital Sign ไปยัง API
        public async Task<bool> AddVitalSignAsync(VitalSign vitalSign)
        {
            var client = _httpClientFactory.CreateClient("VitalSignApi");
            var response = await client.PostAsJsonAsync("VitalSign", vitalSign);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateVitalSignAsync(VitalSign vitalSign)
        {
            // แปลงค่า null เป็น 0
            vitalSign.Systolic ??= 0;
            vitalSign.Diastolic ??= 0;
            vitalSign.Pulse ??= 0;
            vitalSign.Sp02 ??= 0;
            vitalSign.RespiratoryRate ??= 0;
            vitalSign.Height ??= 0;
            vitalSign.Weight ??= 0;
            vitalSign.Bmi ??= 0;
            vitalSign.Temp ??= 0;

            // บังคับ RecordedAt เป็นเวลาปัจจุบัน
            vitalSign.RecordedAt = DateTime.Now;


            var client = _httpClientFactory.CreateClient("VitalSignApi");
            var response = await client.PutAsJsonAsync("VitalSign", vitalSign);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<VitalSign>> GetVitalSignsByHnAsync(string? hn)
        {
            if (string.IsNullOrEmpty(hn))
                return await GetVitalSignsAsync();

            var client = _httpClientFactory.CreateClient("VitalSignApi");

            try
            {
                var result = await client.GetFromJsonAsync<List<VitalSign>>($"VitalSign/byhn?hn={Uri.EscapeDataString(hn)}");
                return result ?? new List<VitalSign>();
            }
            catch (HttpRequestException ex)
            {
                // Log error และ return empty list
                Console.WriteLine($"Error searching for HN {hn}: {ex.Message}");
                return new List<VitalSign>();
            }
        }




    }
}
