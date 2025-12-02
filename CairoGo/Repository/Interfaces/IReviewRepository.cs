using CairoGo.Models.Entity;
using CairoGo.Models.Responsemodel;
using CairoGo.Repository.Implementations;

namespace CairoGo.Repository.Interfaces
{
    public interface IReviewRepository : IBaseRepo<Review>
    {
        // Get all reviews for a place
        Task<List<Review>> GetPlaceReviewsAsync(Guid placeId);

        // Get user's review for a place (one review per user per place)
        Task<Review?> GetUserReviewForPlaceAsync(Guid userId, Guid placeId);

        // Get all reviews by a user
        Task<List<Review>> GetUserReviewsAsync(Guid userId);

        // Get place rating statistics
        Task<PlaceRatingStats> GetPlaceRatingStatsAsync(Guid placeId);

        // Check if user already reviewed a place
        Task<bool> HasUserReviewedPlaceAsync(Guid userId, Guid placeId);

        // Update review
        Task<bool> UpdateReviewAsync(Review review);

        // Increment helpful count
        Task<bool> IncrementHelpfulCountAsync(Guid reviewId);
    }
}
