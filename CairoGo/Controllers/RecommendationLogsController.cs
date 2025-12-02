using CairoGo.Models.Entity;
using CairoGo.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CairoGo.Controllers
{
    [Route("api/recommendation-logs")]
    [ApiController]
    public class RecommendationLogsController : ControllerBase
    {
        private readonly IRecommendationLogRepository _recommendationLogRepository;

        public RecommendationLogsController(IRecommendationLogRepository recommendationLogRepository)
        {
            _recommendationLogRepository = recommendationLogRepository;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRecommendationLogs(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest("Invalid User ID");
                var logs = await _recommendationLogRepository.GetUserRecommendationLogsAsync(userId);
                if (logs == null || !logs.Any())
                    return NotFound("No recommendation logs found for this user");


                return Ok(logs);
            }
            catch (Exception ex)
            {
              return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}/shown")]
        public async Task <IActionResult> GetShownRecommendations(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest("Invalid User ID");

                // Get shown recommendations
                var logs = await _recommendationLogRepository.GetShownRecommendationsAsync(userId);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}/clicked")]
        public async Task<IActionResult> GetClickedRecommendations(Guid userId)
        {
            try
            {
                // Validate input
                if (userId == Guid.Empty)
                    return BadRequest("Invalid User ID");

                // Get clicked recommendations
                var logs = await _recommendationLogRepository.GetClickedRecommendationsAsync(userId);

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        [HttpGet("user/{userId}/booked")]
        public async Task<IActionResult> GetBookedRecommendations(Guid userId)
        {
            try
            {
                // Validate input
                if (userId == Guid.Empty)
                    return BadRequest("Invalid User ID");

                // Get booked recommendations
                var logs = await _recommendationLogRepository.GetBookedRecommendationsAsync(userId);

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecommendationLog([FromBody] RecommendationLog log)
        {
            try
            {
                // Validate input object
                if (log == null)
                    return BadRequest("Invalid data");

                // Validate required fields
                if (log.UserId == Guid.Empty || log.PlaceId == Guid.Empty)
                    return BadRequest("Invalid User ID or Place ID");

                // Save to database
                await _recommendationLogRepository.AddAsync(log);

                // Return 201 Created with location header
                return CreatedAtAction(nameof(GetUserRecommendationLogs),new { userId = log.UserId },log);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{logId}/mark-shown")]
        public async Task<IActionResult> MarkAsShown(Guid logId)
        {
            try
            {
                // Validate input
                if (logId == Guid.Empty)
                    return BadRequest("Invalid Log ID");

                // Update the flag
                await _recommendationLogRepository.MarkAsShownAsync(logId);

                // Return 204 - successful update with no content
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{logId}/mark-clicked")]
        public async Task<IActionResult> MarkAsClicked(Guid logId)
        {
            try
            {
                // Validate input
                if (logId == Guid.Empty)
                    return BadRequest("Invalid Log ID");

                // Update the flags (clicked + shown)
                await _recommendationLogRepository.MarkAsClickedAsync(logId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{logId}/mark-booked")]
        public async Task<IActionResult> MarkAsBooked(Guid logId)
        {
            try
            {
                // Validate input
                if (logId == Guid.Empty)
                    return BadRequest("Invalid Log ID");

                // Update all flags (booked + clicked + shown)
                await _recommendationLogRepository.MarkAsBookedAsync(logId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("user/{userId}/count")]
        public async Task<IActionResult> GetTotalRecommendationCount(Guid userId)
        {
            try
            {
                // Validate input
                if (userId == Guid.Empty)
                    return BadRequest("Invalid User ID");

                // Get count from database
                var count = await _recommendationLogRepository.GetTotalRecommendationCountAsync(userId);

                // Return formatted response
                return Ok(new { userId, totalRecommendations = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("analytics/ctr/{modelVersion}")]
        public async Task<IActionResult> GetClickThroughRate(string modelVersion)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(modelVersion))
                    return BadRequest("Invalid Model Version");

                // Calculate CTR
                var ctr = await _recommendationLogRepository.GetClickThroughRateAsync(modelVersion);

                // Return formatted response with both string and numeric value
                return Ok(new
                {
                    modelVersion,
                    clickThroughRate = $"{ctr:F2}%", // Formatted string (e.g., "15.50%")
                    ctrValue = ctr                    // Raw numeric value
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpDelete("{logId}")]
        public async Task<IActionResult> DeleteRecommendationLog(Guid logId)
        {
            try
            {
                // Validate input
                if (logId == Guid.Empty)
                    return BadRequest("Invalid Log ID");

                // Check if log exists
                var log = await _recommendationLogRepository.GetByIdAsync(logId);
                if (log == null)
                    return NotFound("Recommendation log not found");

                // Delete from database
                await _recommendationLogRepository.DeleteAsync(logId);

                // Return 204 - successful deletion
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

    }
}
