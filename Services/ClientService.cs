using Logistics.Models;

namespace Logistics.Services
{
    public class ClientService
    {
        private readonly HttpClient _httpClient;
        public ClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<Client>> GetClientsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/clients");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Client>>() ?? new List<Client>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error (Clients): {ex.Message}");
                throw new Exception("Failed to load clients. Please try again.");
            }
        }
        public async Task<Client?> GetClientAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/clients/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<Client>();
        }
        public async Task PostClientAsync(Client client)
        {
            var response = await _httpClient.PostAsJsonAsync("api/clients", client);
            response.EnsureSuccessStatusCode();
        }
        public async Task UpdateClientAsync(int id, Client client)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/clients/{id}", client);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeleteClientAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/clients/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
