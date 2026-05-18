using System.Text.Json;

namespace Logistics.Services
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> ConvertCurrencyAsync(
            string fromCurrency,
            string toCurrency,
            decimal amount)
        {
            string url =
                $"https://api.frankfurter.app/latest?amount={amount}&from={fromCurrency}&to={toCurrency}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return 0;
            }

            string json = await response.Content.ReadAsStringAsync();

            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement rates =
                document.RootElement.GetProperty("rates");

            return rates
                .GetProperty(toCurrency.ToUpper())
                .GetDecimal();
        }
    }
    }
