using CairoGo.DTOs.ReviewsDtos;
using CairoGo.Models.Entity;
using CairoGo.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CairoGo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewsController(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        // api/Reviews/Place/{placeId}
        [HttpGet("place/{placeId}")]
        public async Task<IActionResult> GetPlaceReviews(Guid placeId)
        {
            try
            {
                if (placeId == Guid.Empty)
                    return BadRequest("Invalid Place Id");
                var Reviews = await _reviewRepository.GetPlaceReviewsAsync(placeId);
                if (Reviews == null)
                    return NotFound();
                return Ok(Reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // api/review/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserReviews(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest("invalid User Id");
                var Reviews = await _reviewRepository.GetUserReviewsAsync(userId);
                if (Reviews == null)
                    return NotFound();
                return Ok(Reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // api/review/check?userId=...&placeId=.
        [HttpGet("check")]
        public async Task<IActionResult> CheckUserReview([FromQuery] Guid userId, [FromQuery] Guid placeId)
        {
            try
            {
                if (userId == Guid.Empty || placeId == Guid.Empty)
                    return BadRequest("Invalid Place Id Or User Id");
                var reviews = await _reviewRepository.GetUserReviewForPlaceAsync(userId, placeId);
                if (reviews == null)
                    return NotFound();
                return Ok(new { reviews });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("place/{placeId}/stats")]
        public async Task<IActionResult> GetPlaceRatingStats(Guid placeId)
        {
            try
            {
                if (placeId == Guid.Empty)
                    return BadRequest("Invalid Place ID");

                var stats = await _reviewRepository.GetPlaceRatingStatsAsync(placeId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] ReviewDto dto)
        {
            try
            {
                if (dto.UserId == Guid.Empty || dto.PlaceId == Guid.Empty)
                    return BadRequest("Invalid User ID or Place ID");

                if (dto.Rating < 0 || dto.Rating > 5)
                    return BadRequest("Rating must be between 0 and 5");

                var hasReviewed = await _reviewRepository.HasUserReviewedPlaceAsync(dto.UserId, dto.PlaceId);
                if (hasReviewed)
                    return BadRequest("You have already reviewed this place. Use PUT to update your review.");

                var review = new Review
                {
                    UserId = dto.UserId,
                    PlaceId = dto.PlaceId,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    PhotosJson = dto.PhotosJson,
                    VisitDate = dto.VisitDate
                };

                await _reviewRepository.AddAsync(review);
                

                return CreatedAtAction(nameof(GetPlaceReviews), new { placeId = dto.PlaceId }, review);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize]

        public async Task<IActionResult> UpdateReview(Guid id, [FromBody] ReviewUpdateDto dto)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Invalid Id");
                if (dto.Rating < 0 || dto.Rating > 5)
                    return BadRequest("Rating must be between 0 and 5");
                var ExistingReview = await _reviewRepository.GetByIdAsync(id);
                if (ExistingReview == null)
                    return NotFound("Review not found");
                if (dto.UserId.HasValue && ExistingReview.UserId != dto.UserId)
                    return Forbid("You can only edit your own reviews");
                var review = new Review()
                {
                    ReviewId = id,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    PhotosJson = dto.PhotosJson,
                    VisitDate = dto.VisitDate
                };
                var updated = await _reviewRepository.UpdateReviewAsync(review);
                if (!updated)
                    return NotFound("Review Not Found");
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("{id}/helpful")]
        [Authorize]

        public async Task<IActionResult> MarkAsHelpful(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Invalid Review ID");

                var incremented = await _reviewRepository.IncrementHelpfulCountAsync(id);
                if (!incremented)
                    return NotFound("Review not found");
                

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        [Authorize]

        public async Task<IActionResult> DeleteReview(Guid id, Guid? userId = null)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Invalid Review ID");

                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null)
                    return NotFound("Review not found");

                if (userId.HasValue && review.UserId != userId.Value)
                    return Forbid("You can only delete your own reviews");

                await _reviewRepository.DeleteAsync(id);
                

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }

    }
}
