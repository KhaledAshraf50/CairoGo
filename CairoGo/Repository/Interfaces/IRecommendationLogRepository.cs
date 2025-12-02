using CairoGo.Models.Entity;

namespace CairoGo.Repository.Interfaces
{
    public interface IRecommendationLogRepository : IBaseRepo<RecommendationLog>
    {
        Task<List<RecommendationLog>> GetUserRecommendationLogsAsync(Guid userId); // get all reco for the user 
        Task<RecommendationLog?> GetLogByUserAndPlaceAsync(Guid userId, Guid placeId); // Get a specific recommendation log by user and place
        Task<List<RecommendationLog>> GetShownRecommendationsAsync(Guid userId);  // Get all recommendations that were shown to the user
        Task<List<RecommendationLog>> GetClickedRecommendationsAsync(Guid userId); // Get all recommendations that the user clicked on
        Task<List<RecommendationLog>> GetBookedRecommendationsAsync(Guid userId); //Get all recommendations that resulted in bookings
        Task MarkAsShownAsync(Guid logId);  // Mark a recommendation as shown to the user
        Task MarkAsClickedAsync(Guid logId);// Mark a recommendation as clicked by the user
        Task MarkAsBookedAsync(Guid logId); // Mark a recommendation as booked by the user
        Task<int> GetTotalRecommendationCountAsync(Guid userId); //Get total count of recommendations generated for a user
        Task<double> GetClickThroughRateAsync(string modelVersion); //Calculate Click-Through Rate (CTR) for a specific model version
    }
}
