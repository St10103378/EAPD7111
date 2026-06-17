using System.Net.Http.Json;
using Logistics.Models;

namespace Logistics.Services
{
    public class ContractService
    {
        private readonly HttpClient _httpClient;
        public ContractService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<Contract>> GetContractsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/contracts");

                response.EnsureSuccessStatusCode();

                return await response.Content
                    .ReadFromJsonAsync<List<Contract>>() ?? new List<Contract>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load contracts: {ex.Message}", ex);
            }
        }
        public async Task<Contract?> GetContractAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/contracts/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<Contract>();
        }
        public async Task PostContractAsync(Contract contract)
        {
            var response = await _httpClient.PostAsJsonAsync("api/contracts", contract);
            response.EnsureSuccessStatusCode();
        }
        public async Task UpdateContractAsync(int id, Contract contract)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/contracts/{id}", contract);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeleteContractAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/contracts/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
