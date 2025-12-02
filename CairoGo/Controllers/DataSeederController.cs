using CairoGo.Data;
using CairoGo.Models.DbContextApp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CairoGo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataSeederController : ControllerBase
    {
        private readonly CairoGoDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DataSeederController(CairoGoDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            return Ok(new
            {
                activityTypes = await _context.ActivityTypes.CountAsync(),
                places = await _context.Places.CountAsync(),
                vibeTags = await _context.PlaceVibeTags.CountAsync(),
                operatingHours = await _context.PlaceOperatingHours.CountAsync(),
                trendingTags = await _context.TrendingTags.CountAsync()
            });
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedData([FromQuery] string fileName = "CairoGoPlaces.xlsx")
        {
            try
            {
                var filePath = Path.Combine(_env.ContentRootPath, "Data", fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { error = $"File not found: {filePath}" });
                }

                var seeder = new DataSeeder(_context, filePath);
                await seeder.SeedAllAsync();

                return Ok(new
                {
                    message = "✅ Data seeded successfully",
                    status = await GetCurrentStatus()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Seeding failed",
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearData()
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                Console.WriteLine("🗑️  Clearing existing data...");

                // Delete in reverse order of dependencies
                var placeActivityTypes = await _context.Database
                    .ExecuteSqlRawAsync("DELETE FROM PlaceActivityTypes");

                var vibeTags = await _context.Database
                    .ExecuteSqlRawAsync("DELETE FROM PlaceVibeTags");

                var operatingHours = await _context.Database
                    .ExecuteSqlRawAsync("DELETE FROM PlaceOperatingHours");

                var trendingTagsPlaces = await _context.Database
                    .ExecuteSqlRawAsync("DELETE FROM PlaceTrendingTags");

                var trendingTags = await _context.Database
                    .ExecuteSqlRawAsync("DELETE FROM TrendingTags");

                var places = await _context.Database
                    .ExecuteSqlRawAsync("DELETE FROM Places");

                var activityTypes = await _context.Database
                    .ExecuteSqlRawAsync("DELETE FROM ActivityTypes");

                await transaction.CommitAsync();

                Console.WriteLine("✅ All data cleared successfully");

                return Ok(new
                {
                    message = "✅ Data cleared successfully",
                    deletedRecords = new
                    {
                        placeActivityTypes,
                        vibeTags,
                        operatingHours,
                        trendingTagsPlaces,
                        trendingTags,
                        places,
                        activityTypes
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Clear failed",
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPost("reseed")]
        public async Task<IActionResult> ReseedData([FromQuery] string fileName = "CairoGoPlaces.xlsx")
        {
            try
            {
                // Clear existing data first
                await ClearDataInternal();

                // Then seed new data
                var filePath = Path.Combine(_env.ContentRootPath, "Data", fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { error = $"File not found: {filePath}" });
                }

                var seeder = new DataSeeder(_context, filePath);
                await seeder.SeedAllAsync();

                return Ok(new
                {
                    message = "✅ Data reseeded successfully",
                    status = await GetCurrentStatus()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Reseeding failed",
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        private async Task ClearDataInternal()
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM PlaceActivityTypes");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM PlaceVibeTags");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM PlaceOperatingHours");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM PlaceTrendingTags");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM TrendingTags");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Places");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ActivityTypes");

            await transaction.CommitAsync();
        }

        private async Task<object> GetCurrentStatus()
        {
            return new
            {
                activityTypes = await _context.ActivityTypes.CountAsync(),
                places = await _context.Places.CountAsync(),
                vibeTags = await _context.PlaceVibeTags.CountAsync(),
                operatingHours = await _context.PlaceOperatingHours.CountAsync(),
                trendingTags = await _context.TrendingTags.CountAsync()
            };
        }

        [HttpGet("analyze")]
        public IActionResult AnalyzeExcelFile([FromQuery] string fileName = "CairoGoPlaces.xlsx")
        {
            try
            {
                var filePath = Path.Combine(_env.ContentRootPath, "Data", fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { error = $"File not found: {filePath}" });
                }

                ExcelReader.AnalyzeExcelFile(filePath);

                return Ok(new { message = "Check console for analysis output" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Analysis failed",
                    message = ex.Message
                });
            }
        }
    }
}