using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class OpenAIService
{
    private readonly HttpClient _client;
    private const string API_URL = "https://api.groq.com/openai/v1/chat/completions";
    private readonly string _apiKey;

    public OpenAIService(HttpClient client, IConfiguration configuration)
    {
        _client = client;
        _apiKey = configuration["OpenAI:ApiKey"];

        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new ArgumentException("OpenAI API key is missing. Check your configuration.");
        }

        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetResponse()
    {
        var requestBody = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[]
            {
                new { role = "system", content = "You are a job recommendation assistant." },
                new { role = "user", content = "Find jobs for a software engineer." }
            },
            max_tokens = 100
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync(API_URL, content);

        if (!response.IsSuccessStatusCode)
        {
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"API request failed: {response.StatusCode} - {errorMessage}");
        }

        return await response.Content.ReadAsStringAsync();
    }
}
