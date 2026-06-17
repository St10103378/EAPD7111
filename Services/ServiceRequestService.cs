using Logistics.Models;

namespace Logistics.Services
{
    public class ServiceRequestService
    {
        private readonly HttpClient _httpClient;
        public ServiceRequestService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<ServiceRequest>> GetServiceRequestsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/servicerequests");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ServiceRequest>>() ?? new List<ServiceRequest>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error (ServiceRequests): {ex.Message}");
                throw new Exception("Failed to load service requests.");
            }
        }
        public async Task<ServiceRequest?> GetServiceRequestAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/servicerequests/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ServiceRequest>();
        }
        public async Task PostServiceRequestAsync(ServiceRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/servicerequests", request);
            response.EnsureSuccessStatusCode();
        }
        public async Task UpdateServiceRequestAsync(int id, ServiceRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/servicerequests/{id}", request);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeleteServiceRequestAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/servicerequests/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
