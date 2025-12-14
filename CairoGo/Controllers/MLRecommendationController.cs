using CairoGo.DTOs.MLIntegration;
using CairoGo.Mappings;
using CairoGo.Models.Entity;
using CairoGo.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace CairoGo.Controllers
{
    [Route("api/ml-recommendations")]
    [ApiController]
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

        // يجيب Preference Profile بتاع اليوزر ويبعته للـ ML ويرجع Recommendations

        [HttpPost("generate")]
        [Authorize]
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

    
        // يرسل feedback للـ ML (click, bookmark, view)

        [HttpPost("feedback")]
        [Authorize]
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

    
        // يولد 4 خطط بديلة (Alternative Plans) من الـ ML
        // الـ ML بيستخدم عدد الأيام اللي اتحفظ من الكويز
 
        [HttpPost("plans/generate")]
        [Authorize]
        public async Task<IActionResult> GenerateAlternativePlans()
        {
            try
            {
                _logger.LogInformation("Generating alternative plans from ML");

                // ابعت للـ ML (مش محتاج parameters - الـ ML بيستخدم stored data)
                var plans = await _mlService.GenerateAlternativePlansAsync();

                if (plans == null || !plans.Any())
                    return Ok(new
                    {
                        message = "No plans generated. Make sure recommendations were generated first.",
                        plans = new Dictionary<string, List<MLPlanPlaceDto>>()
                    });

                return Ok(new
                {
                    message = "Alternative plans generated successfully",
                    totalPlans = plans.Count,
                    plans
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating alternative plans");
                return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
            }
        }

        // Helper method: يحفظ Recommendations في الـ Database (Optional)
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
                            ModelVersion = "ML_Python_v2.0",
                            Score = rec.Final_Score,
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