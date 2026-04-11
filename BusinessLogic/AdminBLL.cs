using DataTransferObject;
using System;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BusinessLogic
{
    public class AdminBLL
    {
        private readonly HttpClient _httpClient;

        public AdminBLL()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(AppConfig.BaseUrl);
        }

        public async Task<GetUsersAdminResponseDTO> GetUsersAsync(int page = 1, int limit = 20)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionManager.Token);
            var response = await _httpClient.GetAsync($"/admin/users?page={page}&limit={limit}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GetUsersAdminResponseDTO>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API call failed with status code {response.StatusCode}: {errorContent}");
        }

        public async Task BanUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId is required");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionManager.Token);
            var response = await _httpClient.PutAsync($"/admin/users/{userId}/ban", null);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API call failed with status code {response.StatusCode}: {errorContent}");
            }
        }

        public async Task UnbanUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId is required");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionManager.Token);
            var response = await _httpClient.PutAsync($"/admin/users/{userId}/unban", null);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API call failed with status code {response.StatusCode}: {errorContent}");
            }
        }

        public async Task UpdateUserRestrictionsAsync(string userId, List<string> restrictions, string reason)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId is required");

            var payload = new UpdateUserRestrictionsRequestDTO
            {
                Restrictions = restrictions ?? new List<string>(),
                Reason = reason
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionManager.Token);
            var response = await _httpClient.PutAsync($"/admin/users/{userId}/restrictions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API call failed with status code {response.StatusCode}: {errorContent}");
            }
        }

        public async Task<DashboardStatsDTO> GetDashboardStatsAsync()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionManager.Token);
            var response = await _httpClient.GetAsync("/admin/stats");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DashboardStatsDTO>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API call failed with status code {response.StatusCode}: {errorContent}");
        }

        public async Task<TimeSeriesResponseDTO> GetTimeSeriesStatsAsync(string metric, string period = "daily", DateTime? startDate = null, DateTime? endDate = null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionManager.Token);
            var queryParts = new List<string>
            {
                $"metric={Uri.EscapeDataString(metric)}",
                $"period={Uri.EscapeDataString(period)}"
            };
            if (startDate.HasValue) queryParts.Add($"start_date={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue) queryParts.Add($"end_date={endDate.Value:yyyy-MM-dd}");

            var url = $"/admin/stats/timeseries?{string.Join("&", queryParts)}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TimeSeriesResponseDTO>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API call failed with status code {response.StatusCode}: {errorContent}");
        }

        public async Task<PopularTagsResponseDTO> GetPopularTagsAsync()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionManager.Token);
            var response = await _httpClient.GetAsync("/admin/analytics/popular-tags");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PopularTagsResponseDTO>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API call failed with status code {response.StatusCode}: {errorContent}");
        }
    }
}