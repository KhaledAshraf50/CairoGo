using CairoGo.Models.Entity;
using CairoGo.Models.ENums;
using CairoGo.Models.Responsemodel;

namespace CairoGo.Repository.Interfaces
{
    public interface IInteractionRepository : IBaseRepo<Interaction>
    {
        // Get user's interactions with a specific place
        Task<List<Interaction>> GetUserPlaceInteractionsAsync(Guid userId, Guid placeId);

        // Get all interactions for a place
        Task<List<Interaction>> GetPlaceInteractionsAsync(Guid placeId);

        // Get all interactions by a user
        Task<List<Interaction>> GetUserInteractionsAsync(Guid userId);

        // Get interactions by type
        Task<List<Interaction>> GetInteractionsByTypeAsync(Guid userId, InteractionType type);

        // Check if user has specific interaction with place
        Task<bool> HasInteractionAsync(Guid userId, Guid placeId, InteractionType type);

        // Get interaction statistics for a place
        Task<InteractionStats> GetPlaceInteractionStatsAsync(Guid placeId);

        // Get user's recent interactions
        Task<List<Interaction>> GetRecentInteractionsAsync(Guid userId, int count = 10);

        // Delete specific interaction
        Task<bool> DeleteInteractionAsync(Guid userId, Guid placeId, InteractionType type);
    }
}

