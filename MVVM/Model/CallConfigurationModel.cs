using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using DotNetEnv;

namespace DocStack.MVVM.Model
{
    public class CallConfigurationModel
    {
        private readonly HttpClient _httpClient;

        public CallConfigurationModel()
        {
            _httpClient = new HttpClient();
            ConfigureApiKey();
        }

        private void ConfigureApiKey()
        {
            DotNetEnv.Env.Load();
            string apiKey = Environment.GetEnvironmentVariable("CORE_API_KEY");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<JArray> SearchPapersAsync(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return new JArray();

            string apiUrl = $"https://api.core.ac.uk/v3/search/works?q={Uri.EscapeDataString(searchQuery)}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                JObject jsonResponse = JObject.Parse(responseBody);
                return (JArray)jsonResponse["results"];
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return new JArray();
            }
        }
    }
}