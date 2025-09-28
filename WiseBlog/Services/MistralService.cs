using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace WiseBlog.Services
{
    public class MistralService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _mistralEndpoint = "https://api.mistral.ai/v1/chat/completions";

        public MistralService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Mistral:ApiKey"]; 
            Console.WriteLine(_apiKey);
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            var requestBody = new
            {
                model = "mistral-large-latest",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync(_mistralEndpoint, jsonContent);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"Error: {response.StatusCode} - {responseString}";
            }

            try
            {
                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;
                var text = root.GetProperty("choices")[0]
                               .GetProperty("message")
                               .GetProperty("content")
                               .GetString();

                return text ?? "No response";
            }
            catch
            {
                return "Error parsing response";
            }
        }

        public async Task<string> GenerateBlogAsync(string prompt)
        {
            var requestBody = new
            {
                model = "mistral-large-latest",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync(_mistralEndpoint, jsonContent);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"Error: {response.StatusCode} - {responseString}";
            }

            try
            {
                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;
                var text = root.GetProperty("choices")[0]
                               .GetProperty("message")
                               .GetProperty("content")
                               .GetString();

                return text ?? "No response";
            }
            catch
            {
                return "Error parsing response";
            }
        }
    }
}
