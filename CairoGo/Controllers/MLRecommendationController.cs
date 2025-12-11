using CairoGo.DTOs.MLIntegration;
using CairoGo.Mappings;
using CairoGo.Models.Entity;
using CairoGo.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CairoGo.Controllers
{
    [Route("api/ml-recommendations")]
    [ApiController]
    [Authorize]
    public class MLRecommendationController : ControllerBase
    {
        private readonly IMLRecommenderService _mlService;
        private readonly IPreferenceRepo _preferenceRepo;
        private readonly IPlaceRepository _placeRepo;
        private readonly IRecommendationLogRepository _recLogRepo;
        private readonly ILogger<MLRecommendationController> _logger;

        public MLRecommendationController(
            IMLRecommenderService mlService,
            IPreferenceRepo preferenceRepo,
            IPlaceRepository placeRepo,
            IRecommendationLogRepository recLogRepo,
            ILogger<MLRecommendationController> logger)
        {
            _mlService = mlService;
            _preferenceRepo = preferenceRepo;
            _placeRepo = placeRepo;
            _recLogRepo = recLogRepo;
            _logger = logger;
        }
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateRecommendations([FromBody] GenerateRecommendationsRequestDto dto)
        {
            try
            {
                if (dto.UserId == Guid.Empty)
                    return BadRequest(new { message = "Invalid User ID" });

                // 1. جيب الـ Preference Profile
                var profile = await _preferenceRepo.GetPreferencesByUserIdAsync(dto.UserId);
                if (profile == null)
                    return NotFound(new { message = "User preference profile not found. Please complete the quiz first." });

                // 2. حول الـ Profile لـ ML Format
                var mlAnswers = MLMapper.ToMLAnswers(profile);

                _logger.LogInformation($"Sending {mlAnswers.Count} answers to ML for user {dto.UserId}");

                // 3. ابعت للـ ML
                var recommendations = await _mlService.GetInitialRecommendationsAsync(mlAnswers);

                if (recommendations == null || !recommendations.Any())
                    return Ok(new
                    {
                        message = "No recommendations returned from ML model",
                        recommendations = new List<MLRecommendationDto>()
                    });

                // 4. (Optional) احفظ في RecommendationLogs
                await SaveRecommendationLogs(dto.UserId, recommendations);

                return Ok(new
                {
                    message = "Recommendations generated successfully",
                    count = recommendations.Count,
                    recommendations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating recommendations for user {dto.UserId}");
                return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// يرسل feedback للـ ML (click, bookmark, view)
        /// </summary>
        [HttpPost("feedback")]
        public async Task<IActionResult> SendFeedback([FromBody] MLFeedbackInputDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.ItemName))
                    return BadRequest(new { message = "Place name is required" });

                if (string.IsNullOrWhiteSpace(dto.Action))
                    return BadRequest(new { message = "Action is required (click, bookmark, view)" });

                // Validate action
                var validActions = new[] { "click", "bookmark", "view" };
                if (!validActions.Contains(dto.Action.ToLower()))
                    return BadRequest(new { message = "Invalid action. Must be: click, bookmark, or view" });

                _logger.LogInformation($"Sending feedback: {dto.ItemName} - {dto.Action}");

                // ابعت للـ ML
                var updatedRecommendations = await _mlService.UpdateWithFeedbackAsync(dto.ItemName, dto.Action);

                return Ok(new
                {
                    message = "Feedback sent successfully",
                    count = updatedRecommendations.Count,
                    updatedRecommendations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending feedback for {dto.ItemName}");
                return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// يعمل Itinerary كامل من الـ ML
        /// </summary>
        [HttpPost("itinerary")]
        public async Task<IActionResult> CreateItinerary([FromBody] MLItineraryRequestDto dto)
        {
            try
            {
                if (dto.Days < 1 || dto.Days > 30)
                    return BadRequest(new { message = "Days must be between 1 and 30" });

                _logger.LogInformation($"Creating itinerary for {dto.Days} days");

                // ابعت للـ ML
                var itinerary = await _mlService.CreateItineraryAsync(dto.Days);

                if (itinerary == null || !itinerary.Any())
                    return Ok(new
                    {
                        message = "No itinerary generated",
                        itinerary = new Dictionary<string, List<MLDayPlaceDto>>()
                    });

                return Ok(new
                {
                    message = "Itinerary created successfully",
                    totalDays = itinerary.Count,
                    itinerary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating itinerary for {dto.Days} days");
                return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Helper method: يحفظ Recommendations في الـ Database (Optional)
        /// </summary>
        private async Task SaveRecommendationLogs(Guid userId, List<MLRecommendationDto> recommendations)
        {
            try
            {
                foreach (var rec in recommendations.Take(10)) // احفظ أول 10 بس
                {
                    // لازم تجيب الـ Place من Database عشان تاخد الـ PlaceId
                    var places = await _placeRepo.SearchByNameAsync(rec.Name.ToLower());
                    var place = places?.FirstOrDefault();

                    if (place != null)
                    {
                        var log = new RecommendationLog
                        {
                            UserId = userId,
                            PlaceId = place.PlaceId,
                            ModelVersion = "ML_Python_v1.0",
                            Score = rec.Score,
                            WasShown = false,
                            WasClicked = false,
                            WasBooked = false
                        };

                        await _recLogRepo.AddAsync(log);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save recommendation logs");
                // لا ترجع error - الحفظ optional
            }
        }
    }
}