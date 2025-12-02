using CairoGo.DTOs.ModelVersionDtos;
using CairoGo.Models.Entity;
using CairoGo.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CairoGo.Controllers
{
    [Route("api/model-versions")]
    [ApiController]
    public class ModelVersionsController : ControllerBase
    {
        private readonly IModelVersionRepository _modelVersionRepository;

        public ModelVersionsController(IModelVersionRepository modelVersionRepository)
        {
            _modelVersionRepository = modelVersionRepository;
        }

        // GET: api/modelversions
        [HttpGet]
        public async Task<IActionResult> GetAllModels()
        {
            try
            {
                var models = await _modelVersionRepository.GetAllModelsAsync();
                return Ok(models);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET: api/modelversions/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveModel()
        {
            try
            {
                var model = await _modelVersionRepository.GetActiveModelAsync();
                if (model == null)
                    return NotFound("No active model found");

                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET: api/modelversions/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetModelById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Invalid Model ID");

                var model = await _modelVersionRepository.GetByIdAsync(id);
                if (model == null)
                    return NotFound("Model not found");

                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET: api/modelversions/name/{name}
        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetModelByName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest("Model name is required");

                var model = await _modelVersionRepository.GetModelByNameAsync(name);
                if (model == null)
                    return NotFound("Model not found");

                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET: api/modelversions/type/{type}
        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetModelsByType(string type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(type))
                    return BadRequest("Model type is required");

                var models = await _modelVersionRepository.GetModelsByTypeAsync(type);
                return Ok(models);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET: api/modelversions/{id}/performance
        [HttpGet("{id}/performance")]
        public async Task<IActionResult> GetModelPerformance(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Invalid Model ID");

                var stats = await _modelVersionRepository.GetModelPerformanceAsync(id);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // POST: api/modelversions
        [HttpPost]
        public async Task<IActionResult> CreateModel([FromBody] ModelVersionDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest("Model name is required");

                if (string.IsNullOrWhiteSpace(dto.Type))
                    return BadRequest("Model type is required");

                // Check if model name already exists
                var existing = await _modelVersionRepository.GetModelByNameAsync(dto.Name);
                if (existing != null)
                    return BadRequest("Model with this name already exists");

                var model = new ModelVersion
                {
                    Name = dto.Name,
                    Type = dto.Type,
                    ConfigJson = dto.ConfigJson,
                    DeployedAt = DateTime.UtcNow,
                    IsActive = dto.IsActive
                };

                await _modelVersionRepository.AddAsync(model);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // PUT: api/modelversions/{id}/activate
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateModel(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Invalid Model ID");

                var activated = await _modelVersionRepository.ActivateModelAsync(id);
                if (!activated)
                    return NotFound("Model not found");


                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // PUT: api/modelversions/{id}/deprecate
        [HttpPut("{id}/deprecate")]
        public async Task<IActionResult> DeprecateModel(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Invalid Model ID");

                var deprecated = await _modelVersionRepository.DeprecateModelAsync(id);
                if (!deprecated)
                    return NotFound("Model not found");


                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // PUT: api/modelversions/{id}/metrics
        [HttpPut("{id}/metrics")]
        public async Task<IActionResult> UpdateModelMetrics(Guid id, [FromBody] ModelMetricsDto dto)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Invalid Model ID");

                var updated = await _modelVersionRepository.UpdateModelMetricsAsync(
                    id,
                    dto.AverageCTR,
                    dto.AverageConversion);

                if (!updated)
                    return NotFound("Model not found");



                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // POST: api/modelversions/{id}/predict
        [HttpPost("{id}/predict")]
        public async Task<IActionResult> IncrementPredictionCount(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Invalid Model ID");

                var incremented = await _modelVersionRepository.IncrementPredictionCountAsync(id);
                if (!incremented)
                    return NotFound("Model not found");


                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // DELETE: api/modelversions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModel(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest("Invalid Model ID");

                var model = await _modelVersionRepository.GetByIdAsync(id);
                if (model == null)
                    return NotFound("Model not found");

                // Don't allow deleting active model
                if (model.IsActive)
                    return BadRequest("Cannot delete active model. Deactivate it first.");

                await _modelVersionRepository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
