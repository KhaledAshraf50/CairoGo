using CairoGo.Models.DbContextApp;
using CairoGo.Models.Entity;
using CairoGo.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CairoGo.Repository.Implementations
{
    public class RecommendationLogRepository : Repository<RecommendationLog>, IRecommendationLogRepository
    {
        public RecommendationLogRepository(CairoGoDbContext Db) : base(Db)
        {
        }
        public async Task<List<RecommendationLog>> GetUserRecommendationLogsAsync(Guid userId)
        {
            return await _DbSet.Where(r => r.UserId == userId)
                               .Include(r => r.Place)
                               .OrderByDescending(r => r.GeneratedAt)
                               .ToListAsync();
        }
        public async Task<RecommendationLog?> GetLogByUserAndPlaceAsync(Guid userId, Guid placeId)
        {
             return await _DbSet .FirstOrDefaultAsync(r => r.UserId == userId && r.PlaceId == placeId);
        }
        public async Task<List<RecommendationLog>> GetShownRecommendationsAsync(Guid userId)
        {
            return await _DbSet.Where(r => r.UserId == userId && r.WasShown)
                               .Include(r => r.Place)
                               .OrderByDescending(r => r.GeneratedAt)
                               .ToListAsync();
        }
        public async Task<List<RecommendationLog>> GetClickedRecommendationsAsync(Guid userId)
        {
            return await _DbSet.Where(r => r.UserId == userId && r.WasClicked)
                         .Include(r => r.Place)
                         .OrderByDescending(r => r.GeneratedAt)
                         .ToListAsync();
        }

        public async Task<List<RecommendationLog>> GetBookedRecommendationsAsync(Guid userId)
        {
            return await _DbSet.Where(r => r.UserId == userId && r.WasBooked)
                               .Include(r => r.Place)
                               .OrderByDescending(r => r.GeneratedAt)
                               .ToListAsync();
        }
        public async Task MarkAsShownAsync(Guid logId)
        {
            var log = await _DbSet.FindAsync(logId);
            if(log != null)
                log.WasShown = true;
                
        }

        public async Task<int> GetTotalRecommendationCountAsync(Guid userId)
        {
            return await _DbSet.Where(r => r.UserId == userId).CountAsync();

        }

        public async Task MarkAsBookedAsync(Guid logId)
        {
            var log = await _DbSet.FindAsync(logId);
            if(log != null)
            {
                log.WasBooked = true;
                log.WasClicked = true;
                log.WasShown = true;
            }
        }

        public async Task MarkAsClickedAsync(Guid logId)
        {
           var log = await _DbSet.FindAsync(logId);
            if(log != null)
            {
                log.WasShown = true;
                log.WasClicked = true;
            }
               
        }

        public async Task<double> GetClickThroughRateAsync(string modelVersion)
        {
            // Get all logs for this model version that were shown
            var logs = await _DbSet.Where(r => r.ModelVersion == modelVersion && r.WasShown).ToListAsync();

            // If no impressions, CTR is 0
            if (!logs.Any())
                return 0;

            // Count how many were clicked
            var clickedCount = logs.Count(r => r.WasClicked);


            // Calculate percentage
            return (double)clickedCount / logs.Count * 100;
        }
    }
}
