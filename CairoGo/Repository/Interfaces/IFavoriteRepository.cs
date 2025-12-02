using CairoGo.Models.Entity;

namespace CairoGo.Repository.Interfaces
{
    public interface IFavoriteRepository : IBaseRepo<Favorite>
    {
        Task<List<Favorite>> GetUserFavoritesAsync(Guid userId); // get all fav places
        Task<bool> IsFavoritedAsync(Guid userId, Guid placeId);  // check
        Task<Favorite?> GetByUserAndPlaceAsync(Guid userId, Guid placeId); //get fav by user and palce id
        Task<bool> RemoveFavoriteAsync(Guid userId, Guid placeId); // remove
    }
}
