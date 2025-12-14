using CairoGo.DTOs.MLIntegration;

namespace CairoGo.Repository.Interfaces
{
    public interface IMLRecommenderService
    {

        Task<List<MLRecommendationDto>> GetInitialRecommendationsAsync(List<string> answers);
        Task<List<MLRecommendationDto>> UpdateWithFeedbackAsync(string placeName, string action);
        Task<Dictionary<string, List<MLPlanPlaceDto>>> GenerateAlternativePlansAsync();
    }
}
