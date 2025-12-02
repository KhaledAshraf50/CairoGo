using CairoGo.Models.DbContextApp;
using CairoGo.Models.Entity;
using CairoGo.Models.ENums;
using CairoGo.Models.Responsemodel;
using CairoGo.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CairoGo.Repository.Implementations
{
    public class InteractionRepository : Repository<Interaction>, IInteractionRepository
    {
        public InteractionRepository(CairoGoDbContext Db) : base(Db)
        {
        }


        public async Task<List<Interaction>> GetInteractionsByTypeAsync(Guid userId, InteractionType type)
        {
            return await _DbSet.Where(i => i.UserId == userId && i.Type== type)
                               .Include(i => i.Place)
                               .OrderByDescending(i => i.CreatedAt)
                               .ToListAsync();
        }


        public async Task<InteractionStats> GetPlaceInteractionStatsAsync(Guid placeId)
        {
           var interactions = await _DbSet.Where(i => i.PlaceId == placeId)
                                          .Select(i => i.Type)
                                          .ToListAsync();
            return new InteractionStats()
            {
                TotalFavorites = interactions.Count(i => i == InteractionType.AddToFavorite),
                TotalViews = interactions.Count(i => i == InteractionType.ViewDetails)
            };
        }

        public async Task<List<Interaction>> GetRecentInteractionsAsync(Guid userId, int count = 10)
        {
           return await _DbSet.Where(i => i.UserId == userId)
                        .Include(i => i.Place)
                        .OrderByDescending(i => i.CreatedAt)
                        .Take(count)
                        .ToListAsync();
        }

        public async Task<List<Interaction>> GetUserInteractionsAsync(Guid userId)
        {
            return await _DbSet.Where(i => userId == i.UserId)
                .Include(i => i.Place)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Interaction>> GetUserPlaceInteractionsAsync(Guid userId, Guid placeId)
        {
            return await _DbSet.Where(i => i.UserId ==userId && i.PlaceId == placeId)
                               .OrderByDescending(i => i.CreatedAt)
                               .ToListAsync();
        }

        public async Task<bool> HasInteractionAsync(Guid userId, Guid placeId, InteractionType type)
        {
            return await _DbSet.AnyAsync(i => i.UserId ==  userId && i.PlaceId == placeId && i.Type == type);
        }
        public async Task<bool> DeleteInteractionAsync(Guid userId, Guid placeId, InteractionType type)
        {
            var interaction = await _DbSet.FirstOrDefaultAsync(i => i.UserId == userId && i.PlaceId == placeId && i.Type == type);
            if(interaction == null)
                return false;
            _DbSet.Remove(interaction);
            return true;
        }

        public async Task<List<Interaction>> GetPlaceInteractionsAsync(Guid placeId)
        {
            return await _DbSet.Where(i => i.PlaceId == placeId)
                    .Include(i => i.User)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
        }
    }
}
