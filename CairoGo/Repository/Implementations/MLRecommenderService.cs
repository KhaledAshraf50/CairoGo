using CairoGo.DTOs.MLIntegration;
using CairoGo.Repository.Interfaces;
using System.Text;
using System.Text.Json;

namespace CairoGo.Repository.Implementations
{
    public class MLRecommenderService : IMLRecommenderService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MLRecommenderService> _logger;

        public MLRecommenderService(HttpClient httpClient, ILogger<MLRecommenderService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<MLRecommendationDto>> GetInitialRecommendationsAsync(List<string> answers)
        {
            try
            {
                var requestBody = new MLQuizInputDto { Answers = answers };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/recommend/initial", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"ML API returned {response.StatusCode}: {errorContent}");
                    return new List<MLRecommendationDto>();
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var recommendations = JsonSerializer.Deserialize<List<MLRecommendationDto>>(responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return recommendations ?? new List<MLRecommendationDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling ML initial recommendations API");
                return new List<MLRecommendationDto>();
            }
        }

        public async Task<List<MLRecommendationDto>> UpdateWithFeedbackAsync(string placeName, string action)
        {
            try
            {
                var requestBody = new MLFeedbackInputDto
                {
                    ItemName = placeName,
                    Action = action
                };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/recommend/update", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"ML API returned {response.StatusCode}: {errorContent}");
                    return new List<MLRecommendationDto>();
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var recommendations = JsonSerializer.Deserialize<List<MLRecommendationDto>>(responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return recommendations ?? new List<MLRecommendationDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling ML feedback API");
                return new List<MLRecommendationDto>();
            }
        }

        public async Task<Dictionary<string, List<MLPlanPlaceDto>>> GenerateAlternativePlansAsync()
        {
            try
            {
                // POST request with empty body - ML uses stored trip days from quiz
                var content = new StringContent("{}", Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/plan/generate", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"ML API returned {response.StatusCode}: {errorContent}");
                    return new Dictionary<string, List<MLPlanPlaceDto>>();
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"ML Plans Response: {responseJson}");

                var plans = JsonSerializer.Deserialize<Dictionary<string, List<MLPlanPlaceDto>>>(responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return plans ?? new Dictionary<string, List<MLPlanPlaceDto>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling ML generate plans API");
                return new Dictionary<string, List<MLPlanPlaceDto>>();
            }
        }
    }
}