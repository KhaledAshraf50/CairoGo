using CairoGo.DTOs.InteractionDtos;
using CairoGo.Models.Entity;
using CairoGo.Models.ENums;
using CairoGo.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CairoGo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class InteractionsController : ControllerBase
    {
        private readonly IInteractionRepository _interactionRepository;

        public InteractionsController(IInteractionRepository interactionRepository)
        {
            _interactionRepository = interactionRepository;
        }

        // GET: api/interactions/place/{placeId}
        [HttpGet("place/{placeId}")]
        public async Task<IActionResult> GetPlaceInteractions(Guid placeId)
        {
            try
            {
                if(placeId == Guid.Empty)
                    return BadRequest("Invalid Place ID");
                var Interactions = await _interactionRepository.GetPlaceInteractionsAsync(placeId);
                if (Interactions == null)
                    return NotFound();
                return Ok(Interactions);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        //GET: api/interactions/place/{placeId}/stats
        [HttpGet("place/{placeId}/stats")]
        public async Task<IActionResult> GetPlaceInteractionStats(Guid placeId)
        {
            try
            {
                if (placeId == Guid.Empty)
                    return BadRequest("Invalid Place ID");
                var InteractionsStats= await _interactionRepository.GetPlaceInteractionStatsAsync(placeId);
                if(InteractionsStats == null)
                    return NotFound();
                return Ok(InteractionsStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET: api/interactions/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserInteractions(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest("Invalid Place ID");
                var UserInteractions = await _interactionRepository.GetUserInteractionsAsync(userId);
                if (UserInteractions == null)
                    return NotFound();
                return Ok(UserInteractions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET: api/interactions/check?userId=...& placeId=...& type=
        [HttpGet("check")]
        public async Task<IActionResult> CheckUserInteraction(
            [FromQuery] Guid userId,
            [FromQuery] Guid placeId,
            [FromQuery] InteractionType type)
        {
            try
            {
                if (userId == Guid.Empty || placeId == Guid.Empty)
                    return BadRequest("Invalid User ID or Place ID");

                var hasInteraction = await _interactionRepository.HasInteractionAsync(userId, placeId, type);

                    return Ok(new { hasInteraction, type = type.ToString() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        // POST: api/interactions
        [HttpPost]
        public async Task<IActionResult> CreateInteration([FromBody] InteractionDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid data");

                if (dto.UserId == Guid.Empty || dto.PlaceId == Guid.Empty)
                    return BadRequest("Invalid User Id Or Place Id");
                if (dto.Type == InteractionType.AddToFavorite)
                {
                    var existed= await _interactionRepository.HasInteractionAsync(dto.UserId , dto.PlaceId, dto.Type);
                    if (existed)
                        return BadRequest("User already added this place to favorites");
                }
                var interaction = new Interaction()
                {
                    UserId = dto.UserId,
                    PlaceId = dto.PlaceId,
                    Type = dto.Type,
                    ContextJson = dto.ContextJson
                };
                await _interactionRepository.AddAsync(interaction);
                return CreatedAtAction(nameof(GetPlaceInteractions),new { placeId = dto.PlaceId }, interaction); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE: api/interactions?userId=...&placeId=...&type=...
        [HttpDelete]
        public async Task<IActionResult> DeleteInteraction(
            [FromQuery] Guid userId,
            [FromQuery] Guid placeId,
            [FromQuery] InteractionType type)
        {
            try
            {
                if (userId == Guid.Empty || placeId == Guid.Empty)
                    return BadRequest("Invalid User ID or Place ID");

                var deleted = await _interactionRepository.DeleteInteractionAsync(userId, placeId, type);

                if (!deleted)
                    return NotFound("Interaction not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }




    }
}
