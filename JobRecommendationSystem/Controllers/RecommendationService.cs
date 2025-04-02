using JobRecommendationSystem.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JobRecommendationSystem.Services
{
    public class RecommendationService
    {
        private const string API_URL = "https://api.groq.com/openai/v1/chat/completions";
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationService(HttpClient httpClient, IConfiguration configuration, ILogger<RecommendationService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiKey = configuration["OpenAI:ApiKey"];

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogError("OpenAI API key is missing. Check your appsettings.json.");
                throw new InvalidOperationException("OpenAI API key is missing. Check your appsettings.json.");
            }

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<List<Job>> GetRecommendedJobs(UserProfile user, List<Job> jobs)
        {
            if (user == null || jobs == null || jobs.Count == 0)
            {
                _logger.LogWarning("User profile and job list cannot be null or empty.");
                throw new ArgumentException("User profile and job list cannot be null or empty.");
            }

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = "You are a job recommendation assistant." },
                    new { role = "user", content = $"Suggest jobs for a person with skills: {user.Skills}, experience: {user.Experience} years, and interests: {user.Interests}. Available jobs: {string.Join(", ", jobs.Select(j => j.Title))}." }
                },
                max_tokens = 150
            };

            string jsonRequest = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            try
            {
                var responseContent = await CallOpenAIWithRetry(httpContent);
                var aiResponse = JObject.Parse(responseContent);

                if (!aiResponse.ContainsKey("choices"))
                {
                    _logger.LogError("Invalid API response: 'choices' key missing.");
                    throw new InvalidOperationException("Invalid API response: 'choices' key missing.");
                }

                var messageToken = aiResponse["choices"]?.First?["message"]?["content"];
                var recommendedText = messageToken?.Type == JTokenType.String ? messageToken.ToString() : null;
                var recommendedJobTitles = recommendedText?.Split(',').Select(title => title.Trim()).ToList() ?? new List<string>();

                return jobs.Where(j => recommendedJobTitles.Any(title => title.Equals(j.Title, StringComparison.OrdinalIgnoreCase))).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching recommendations.");
                throw;
            }
        }

        private async Task<string> CallOpenAIWithRetry(HttpContent httpContent, int maxRetries = 3, int delayMs = 2000)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                var response = await _httpClient.PostAsync(API_URL, httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return responseContent;
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("Rate limit exceeded. Retrying in {Delay} seconds...", delayMs / 1000);
                    await Task.Delay(delayMs);
                }
                else
                {
                    _logger.LogError("API request failed: {StatusCode} - {Response}", response.StatusCode, responseContent);
                    throw new HttpRequestException($"API request failed: {response.StatusCode} - {responseContent}");
                }
            }

            _logger.LogError("API request failed after multiple retries.");
            throw new HttpRequestException("API request failed after multiple retries.");
        }
    }
}
