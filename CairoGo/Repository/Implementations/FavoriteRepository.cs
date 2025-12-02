using CairoGo.Models.DbContextApp;
using CairoGo.Models.Entity;
using CairoGo.Repository.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CairoGo.Repository.Implementations
{
    public class FavoriteRepository : Repository<Favorite>, IFavoriteRepository
    {
        public FavoriteRepository(CairoGoDbContext _Db) : base(_Db)
        {
        }
        public async Task<List<Favorite>> GetUserFavoritesAsync(Guid userId)
        {
            return await _DbSet.Where(f => f.UserId == userId)
                               .Include(f => f.Place)
                               .OrderByDescending(f => f.CreatedAt)
                               .ToListAsync();
        }
        public async Task<bool> IsFavoritedAsync(Guid userId, Guid placeId)
        {
            return await _DbSet.AnyAsync(f => f.UserId ==userId && f.PlaceId == placeId);
        }

        public async Task<Favorite?> GetByUserAndPlaceAsync(Guid userId, Guid placeId)
        {
            return await _DbSet.FirstOrDefaultAsync(f => f.UserId == userId && f.PlaceId == placeId);
        }
        public async Task<bool> RemoveFavoriteAsync(Guid userId, Guid placeId)
        {
            var Favorite= await GetByUserAndPlaceAsync(userId, placeId);
            if (Favorite == null)
                return false;
            _DbSet.Remove(Favorite);
            return true;
        }
    }
}
