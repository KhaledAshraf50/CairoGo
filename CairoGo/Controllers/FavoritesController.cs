using CairoGo.DTOs.FavoriteDtos;
using CairoGo.Models.Entity;
using CairoGo.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CairoGo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteRepository _favoriteRepository;

        public FavoritesController(IFavoriteRepository favoriteRepository)
        {
            _favoriteRepository = favoriteRepository;
        }

        // GET: api/favorite/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserFavorites(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest("In Valid Id");
                var FavoritePlaces = await _favoriteRepository.GetUserFavoritesAsync(userId);
                return Ok(FavoritePlaces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET: api/favorites/check?userId=...  &  placeId=...
        [HttpGet("check")]
        public async Task<IActionResult> CheckFavorites([FromQuery] Guid userId , [FromQuery] Guid placeId)
        {
            try
            {
                if (userId == Guid.Empty || placeId == Guid.Empty)
                    return BadRequest("invalid User ID Or Place ID");
                var Isfavorite = await _favoriteRepository.IsFavoritedAsync(userId , placeId);
                return Ok(new {Isfavorite });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        //POST: api/favorites
        [HttpPost]
        public async Task<IActionResult> AddFavorite(FavoriteDto dto)
        {
            try
            {
                if (dto.UserId == Guid.Empty || dto.PlaceId == Guid.Empty)
                    return BadRequest("Invalid User Id Or Place Id");
                var Exists = await _favoriteRepository.IsFavoritedAsync(dto.UserId, dto.PlaceId);
                if (Exists)
                    return BadRequest("Place Already exists");
                var favorite = new Favorite()
                {
                    UserId = dto.UserId,
                    PlaceId = dto.PlaceId,
                };
                await _favoriteRepository.AddAsync(favorite);
                return CreatedAtAction(nameof(GetUserFavorites), new { userId = dto.UserId }, favorite); // q for that ins
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE: api/favorites?userId=...&placeId=...
        [HttpDelete]
        public async Task<IActionResult> RemoveFavorite([FromQuery] Guid UserId , [FromQuery] Guid PlaceId)
        {
            try
            {
                if (UserId == Guid.Empty || PlaceId == Guid.Empty)
                    return BadRequest("Invalid User Id Or Place Id");
                var removed = await _favoriteRepository.RemoveFavoriteAsync(UserId, PlaceId);
                if (!removed)
                    return NotFound("Favorite not found");
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

    }
}
