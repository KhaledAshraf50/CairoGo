using CairoGo.DTOs.TrendingTagDtos;
using CairoGo.Models.Entity;
using CairoGo.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CairoGo.Controllers
{
    [Route("api/trending-tags")]
    [ApiController]
    public class TrendingTagsController : ControllerBase
    {
        private readonly ITrendingTagRepository _trendingTagRepository;

        public TrendingTagsController(ITrendingTagRepository trendingTagRepository)
        {
            _trendingTagRepository = trendingTagRepository;
        }

        [HttpGet("top")]
        public async Task<IActionResult> GetTopTrendingTags([FromQuery] int count = 10)
        {
            try
            {
                var tags = await _trendingTagRepository.GetTopTrendingTagsAsync(count);

                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentTrendingTags()
        {
            try
            {
                var tags = await _trendingTagRepository.GetCurrentTrendingTagsAsync();

                if (tags == null || !tags.Any())
                    return NotFound("No current trending tags found");

                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveTrendingTags()
        {
            try
            {
                var tags = await _trendingTagRepository.GetActiveTrendingTagsAsync();

                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetTrendingTagByName(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return BadRequest("Invalid Tag Name");

                var tag = await _trendingTagRepository.GetTrendingTagByNameAsync(name);

                if (tag == null)
                    return NotFound("Tag not found");

                return Ok(tag);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("period")]
        public async Task<IActionResult> GetTrendingTagsByPeriod(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                    return BadRequest("Start date must be before end date");

                var tags = await _trendingTagRepository.GetTrendingTagsByPeriodAsync(startDate, endDate);

                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("{tagId}")]
        public async Task<IActionResult> GetTrendingTagById(Guid tagId)
        {
            try
            {
                if (tagId == Guid.Empty)
                    return BadRequest("Invalid Tag ID");

                var tag = await _trendingTagRepository.GetByIdAsync(tagId);

                if (tag == null)
                    return NotFound("Tag not found");

                return Ok(tag);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrendingTag([FromBody] TrendingTagDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid data");

                if (string.IsNullOrEmpty(dto.Name))
                    return BadRequest("Tag name is required");

                if (dto.PeriodStart >= dto.PeriodEnd)
                    return BadRequest("Period start must be before period end");

                var exists = await _trendingTagRepository.GetTrendingTagByNameAsync(dto.Name);
                if (exists != null)
                    return BadRequest("Tag with this name already exists");

                var tag = new TrendingTag
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Score = dto.Score,
                    PeriodStart = dto.PeriodStart,
                    PeriodEnd = dto.PeriodEnd
                };

                await _trendingTagRepository.AddAsync(tag);

                return CreatedAtAction(nameof(GetTrendingTagById), new { tagId = tag.TagId }, tag);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{tagId}")]
        public async Task<IActionResult> UpdateTrendingTag(Guid tagId, [FromBody] TrendingTagDto dto)
        {
            try
            {
                if (tagId == Guid.Empty)
                    return BadRequest("Invalid Tag ID");

                if (dto == null)
                    return BadRequest("Invalid data");

                var existing = await _trendingTagRepository.GetByIdAsync(tagId);
                if (existing == null)
                    return NotFound("Tag not found");

                if (dto.PeriodStart >= dto.PeriodEnd)
                    return BadRequest("Period start must be before period end");

                existing.Name = dto.Name;
                existing.Description = dto.Description;
                existing.Score = dto.Score;
                existing.PeriodStart = dto.PeriodStart;
                existing.PeriodEnd = dto.PeriodEnd;

                await _trendingTagRepository.UpdateAsync(existing);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{tagId}/score")]
        public async Task<IActionResult> UpdateTagScore(Guid tagId, [FromBody] UpdateTagScoreDto dto)
        {
            try
            {
                if (tagId == Guid.Empty)
                    return BadRequest("Invalid Tag ID");

                if (dto == null)
                    return BadRequest("Invalid data");

                if (dto.Score < 0)
                    return BadRequest("Score cannot be negative");

                var success = await _trendingTagRepository.UpdateTagScoreAsync(tagId, dto.Score);

                if (!success)
                    return NotFound("Tag not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpDelete("{tagId}")]
        public async Task<IActionResult> DeleteTrendingTag(Guid tagId)
        {
            try
            {
                if (tagId == Guid.Empty)
                    return BadRequest("Invalid Tag ID");

                await _trendingTagRepository.DeleteAsync(tagId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}

