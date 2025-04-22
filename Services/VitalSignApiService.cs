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
            var client = _httpClientFactory.CreateClient("VitalSignApi");
            // "VitalSignApi" มี BaseAddress = http://localhost:55/api/
            var response = await client.PutAsJsonAsync("VitalSign", vitalSign);
            return response.IsSuccessStatusCode;
        }



    }
}
