using CairoGo.Models.Entity;

namespace CairoGo.Repository.Interfaces
{
    public interface ITrendingTagRepository : IBaseRepo<TrendingTag>
    {
    
        Task<List<TrendingTag>> GetTopTrendingTagsAsync(int count = 10);
        Task<List<TrendingTag>> GetCurrentTrendingTagsAsync();
        Task<TrendingTag?> GetTrendingTagByNameAsync(string name);
        Task<List<TrendingTag>> GetTrendingTagsByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<bool> UpdateTagScoreAsync(Guid tagId, float newScore);
        Task<List<TrendingTag>> GetActiveTrendingTagsAsync();
    }
}
