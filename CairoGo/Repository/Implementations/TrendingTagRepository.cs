using CairoGo.Models.DbContextApp;
using CairoGo.Models.Entity;
using CairoGo.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CairoGo.Repository.Implementations
{
    public class TrendingTagRepository : Repository<TrendingTag>, ITrendingTagRepository
    {
        public TrendingTagRepository(CairoGoDbContext Db) : base(Db)
        {
        }
        public async Task<List<TrendingTag>> GetTopTrendingTagsAsync(int count = 10)
        {
            return await _DbSet
                .OrderByDescending(t => t.Score)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<TrendingTag>> GetCurrentTrendingTagsAsync()
        {
            var now = DateTime.UtcNow;

            return await _DbSet
                .Where(t => t.PeriodStart <= now && t.PeriodEnd >= now)
                .OrderByDescending(t => t.Score)
                .ToListAsync();
        }

        public async Task<TrendingTag?> GetTrendingTagByNameAsync(string name)
        {
            return await _DbSet
                .FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<List<TrendingTag>> GetTrendingTagsByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _DbSet
                .Where(t => t.PeriodStart >= startDate && t.PeriodEnd <= endDate)
                .OrderByDescending(t => t.Score)
                .ToListAsync();
        }

        public async Task<bool> UpdateTagScoreAsync(Guid tagId, float newScore)
        {
            var tag = await GetByIdAsync(tagId);
            if (tag == null)
                return false;

            tag.Score = newScore;
            await UpdateAsync(tag);
            return true;
        }

        public async Task<List<TrendingTag>> GetActiveTrendingTagsAsync()
        {
            var now = DateTime.UtcNow;

            return await _DbSet
                .Where(t => t.PeriodEnd >= now)
                .OrderByDescending(t => t.Score)
                .ToListAsync();
        }
    }
}
