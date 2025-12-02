using CairoGo.Models.DbContextApp;
using CairoGo.Models.Entity;
using CairoGo.Models.ENums;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace CairoGo.Data
{
    public class DataSeeder
    {
        private readonly CairoGoDbContext _context;
        private readonly string _excelFilePath;

        private readonly bool _forceReseed;

        public DataSeeder(CairoGoDbContext context, string excelFilePath, bool forceReseed = false)
        {
            _context = context;
            _excelFilePath = excelFilePath;
            _forceReseed = forceReseed;
        }

        public async Task SeedAllAsync()
        {
            if (!File.Exists(_excelFilePath))
            {
                Console.WriteLine($"❌ Excel file not found: {_excelFilePath}");
                return;
            }

            using var workbook = new XLWorkbook(_excelFilePath);
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Console.WriteLine("🌱 Starting data seeding...\n");

                // Seed in order to respect foreign key constraints
                await SeedActivityTypesAsync();
                Console.WriteLine("💾 Saving ActivityTypes...");
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ ActivityTypes saved\n");

                await SeedPlacesAsync(workbook);
                Console.WriteLine("💾 Saving Places...");
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Places saved\n");

                await SeedPlaceVibeTagsAsync(workbook);
                Console.WriteLine("💾 Saving PlaceVibeTags...");
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ PlaceVibeTags saved\n");

                await SeedPlaceActivityTypesAsync(workbook);
                Console.WriteLine("💾 Saving PlaceActivityTypes...");
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ PlaceActivityTypes saved\n");

                await transaction.CommitAsync();
                Console.WriteLine("✅ Transaction committed");

                Console.WriteLine("\n✅ Data seeding completed successfully!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine($"\n❌❌❌ CRITICAL ERROR ❌❌❌");
                Console.WriteLine($"Message: {ex.Message}");

                var innerEx = ex.InnerException;
                var level = 1;
                while (innerEx != null)
                {
                    Console.WriteLine($"\n--- Inner Exception Level {level} ---");
                    Console.WriteLine($"Type: {innerEx.GetType().Name}");
                    Console.WriteLine($"Message: {innerEx.Message}");
                    Console.WriteLine($"StackTrace: {innerEx.StackTrace}");
                    innerEx = innerEx.InnerException;
                    level++;
                }

                throw;
            }
        }

        private async Task SeedActivityTypesAsync()
        {
            if (!_forceReseed && await _context.ActivityTypes.AnyAsync())
            {
                Console.WriteLine("⏭️  ActivityTypes already seeded. Skipping...");
                return;
            }

            var activityTypes = new List<ActivityType>();

            foreach (ActivityKind kind in Enum.GetValues(typeof(ActivityKind)))
            {
                activityTypes.Add(new ActivityType
                {
                    ActivityTypeId = Guid.NewGuid(),
                    Name = kind,
                    Description = GetActivityTypeDescription(kind)
                });
            }

            await _context.ActivityTypes.AddRangeAsync(activityTypes);
            Console.WriteLine($"✅ Prepared {activityTypes.Count} ActivityTypes");
        }

        private async Task SeedPlacesAsync(XLWorkbook workbook)
        {
            if (!_forceReseed && await _context.Places.AnyAsync())
            {
                Console.WriteLine("⏭️  Places already seeded. Skipping...");
                return;
            }

            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null || worksheet.RowsUsed().Count() <= 1)
            {
                Console.WriteLine("❌ No data found in Excel. Skipping Places seeding...");
                return;
            }

            var headerRow = worksheet.Row(1);
            var headers = new Dictionary<string, int>();

            // Build column index map
            for (int col = 1; col <= headerRow.CellsUsed().Count(); col++)
            {
                var headerValue = headerRow.Cell(col).GetString();
                if (!string.IsNullOrWhiteSpace(headerValue))
                {
                    headers[headerValue.Trim().ToLower()] = col;
                }
            }

            var places = new List<Place>();
            int rowNumber = 0;

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                rowNumber++;
                try
                {
                    var name = GetCellValue(row, headers, "name");
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        Console.WriteLine($"⚠️  Row {rowNumber}: Skipping - No name provided");
                        continue;
                    }

                    var tags = GetCellValue(row, headers, "tags") ?? "";
                    var categoryStr = GetCellValue(row, headers, "category");
                    var photoUrl = GetCellValue(row, headers, "photo url") ??
                                   GetCellValue(row, headers, "photourl") ??
                                   GetCellValue(row, headers, "photo");

                    var place = new Place
                    {
                        PlaceId = Guid.NewGuid(),
                        Name = name,
                        Description = $"Explore {name}, one of Cairo's popular destinations.",
                        FullDescription = $"Discover {name} and enjoy a memorable experience. This location offers unique attractions and activities for all visitors.",
                        District = ExtractDistrict(name),
                        ImageUrl = photoUrl ?? "",
                        Category = ParseEnum<PlaceCategory>(categoryStr, PlaceCategory.Attraction),
                        CostTier = DetermineCostTier(name, tags),
                        IndoorOutdoor = DetermineIndoorOutdoor(tags),
                        BestTimeOfDay = DetermineBestTimeOfDay(tags),
                        AvgVisitDurationMinutes = DetermineAvgVisitDuration(categoryStr, tags),
                        Rating = ParseFloatNullable(GetCellValue(row, headers, "rating")),
                        Latitude = ParseDecimalNullable(GetCellValue(row, headers, "latitude")),
                        Longitude = ParseDecimalNullable(GetCellValue(row, headers, "longitude")),
                        Website = GetCellValue(row, headers, "link"),
                        PhoneNumber = null,
                        AccessibilityInfo = "Please contact venue for accessibility information",
                        ParkingInfo = "Street parking available nearby",
                        BestTimeToVisit = "Year-round, avoid peak summer heat",
                        VisitorsPerYear = 0
                    };

                    places.Add(place);
                    Console.WriteLine($"✓ Row {rowNumber}: {name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error parsing row {rowNumber}: {ex.Message}");
                }
            }

            await _context.Places.AddRangeAsync(places);
            Console.WriteLine($"\n✅ Prepared {places.Count} Places for seeding");
        }

        private async Task SeedPlaceVibeTagsAsync(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null || worksheet.RowsUsed().Count() <= 1)
            {
                Console.WriteLine("⏭️  No data for PlaceVibeTags. Skipping...");
                return;
            }

            var places = await _context.Places.ToListAsync();
            if (!places.Any())
            {
                Console.WriteLine("❌ No places found. Cannot seed PlaceVibeTags.");
                return;
            }

            var headerRow = worksheet.Row(1);
            var headers = new Dictionary<string, int>();

            for (int col = 1; col <= headerRow.CellsUsed().Count(); col++)
            {
                var headerValue = headerRow.Cell(col).GetString();
                if (!string.IsNullOrWhiteSpace(headerValue))
                {
                    headers[headerValue.Trim().ToLower()] = col;
                }
            }

            // Load existing vibe tags to avoid duplicates
            var existingVibeTags = await _context.PlaceVibeTags
                .Select(vt => new { vt.PlaceId, vt.Value })
                .ToListAsync();

            var vibeTags = new List<PlaceVibeTag>();
            int rowNumber = 0;

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                rowNumber++;
                try
                {
                    var placeName = GetCellValue(row, headers, "name");
                    var tagsStr = GetCellValue(row, headers, "tags");

                    if (string.IsNullOrWhiteSpace(placeName) || string.IsNullOrWhiteSpace(tagsStr))
                        continue;

                    var place = places.FirstOrDefault(p =>
                        p.Name.Equals(placeName, StringComparison.OrdinalIgnoreCase));

                    if (place == null)
                        continue;

                    // Extract vibes from tags
                    var vibes = ExtractVibesFromTags(tagsStr);

                    foreach (var vibe in vibes)
                    {
                        // Check if already exists
                        var exists = existingVibeTags.Any(vt =>
                            vt.PlaceId == place.PlaceId && vt.Value == vibe);

                        if (!exists && !vibeTags.Any(vt => vt.PlaceId == place.PlaceId && vt.Value == vibe))
                        {
                            vibeTags.Add(new PlaceVibeTag
                            {
                                PlaceVibeTagId = Guid.NewGuid(),
                                PlaceId = place.PlaceId,
                                Value = vibe
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error parsing PlaceVibeTag row {rowNumber}: {ex.Message}");
                }
            }

            if (vibeTags.Any())
            {
                await _context.PlaceVibeTags.AddRangeAsync(vibeTags);
                Console.WriteLine($"✅ Prepared {vibeTags.Count} PlaceVibeTags");
            }
        }

        private async Task SeedPlaceActivityTypesAsync(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null || worksheet.RowsUsed().Count() <= 1)
            {
                Console.WriteLine("⏭️  No data for PlaceActivityTypes. Skipping...");
                return;
            }

            var places = await _context.Places.Include(p => p.ActivityTypes).ToListAsync();
            var activityTypes = await _context.ActivityTypes.ToListAsync();

            if (!places.Any() || !activityTypes.Any())
            {
                Console.WriteLine("❌ No places or activity types found. Cannot seed PlaceActivityTypes.");
                return;
            }

            var headerRow = worksheet.Row(1);
            var headers = new Dictionary<string, int>();

            for (int col = 1; col <= headerRow.CellsUsed().Count(); col++)
            {
                var headerValue = headerRow.Cell(col).GetString();
                if (!string.IsNullOrWhiteSpace(headerValue))
                {
                    headers[headerValue.Trim().ToLower()] = col;
                }
            }

            int relationshipsAdded = 0;

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                try
                {
                    var placeName = GetCellValue(row, headers, "name");
                    var categoryStr = GetCellValue(row, headers, "category");
                    var tagsStr = GetCellValue(row, headers, "tags");

                    if (string.IsNullOrWhiteSpace(placeName))
                        continue;

                    var place = places.FirstOrDefault(p =>
                        p.Name.Equals(placeName, StringComparison.OrdinalIgnoreCase));

                    if (place == null)
                        continue;

                    // Determine activity types based on category and tags
                    var activityKinds = DetermineActivityTypes(categoryStr, tagsStr);

                    foreach (var activityKind in activityKinds)
                    {
                        var activityType = activityTypes.FirstOrDefault(at => at.Name == activityKind);
                        if (activityType != null && !place.ActivityTypes.Any(at => at.ActivityTypeId == activityType.ActivityTypeId))
                        {
                            place.ActivityTypes.Add(activityType);
                            relationshipsAdded++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error in PlaceActivityTypes: {ex.Message}");
                }
            }

            Console.WriteLine($"✅ Created {relationshipsAdded} PlaceActivityType relationships");
        }

        // ============================================
        // HELPER METHODS
        // ============================================

        private string? GetCellValue(IXLRow row, Dictionary<string, int> headers, string headerKey)
        {
            if (headers.TryGetValue(headerKey.ToLower(), out int colIndex))
            {
                var cellValue = row.Cell(colIndex).GetString();
                return string.IsNullOrWhiteSpace(cellValue) ? null : cellValue.Trim();
            }
            return null;
        }

        private T ParseEnum<T>(string? value, T defaultValue) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (Enum.TryParse<T>(value, true, out var result))
                return result;

            return defaultValue;
        }

        private float? ParseFloatNullable(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (float.TryParse(value, out float result))
                return result;

            return null;
        }

        private decimal? ParseDecimalNullable(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (decimal.TryParse(value, out decimal result))
                return result;

            return null;
        }

        private string ExtractDistrict(string placeName)
        {
            // Extract district from place name or default to Cairo
            var name = placeName.ToLower();

            if (name.Contains("zamalek")) return "Zamalek";
            if (name.Contains("new cairo")) return "New Cairo";
            if (name.Contains("maadi")) return "Maadi";
            if (name.Contains("heliopolis")) return "Heliopolis";
            if (name.Contains("downtown")) return "Downtown";
            if (name.Contains("giza")) return "Giza";
            if (name.Contains("nasr city")) return "Nasr City";

            return "Cairo";
        }

        private CostTier DetermineCostTier(string placeName, string tags)
        {
            var combined = (placeName + " " + tags).ToLower();

            // Free/Low cost indicators
            if (combined.Contains("park") || combined.Contains("square") ||
                combined.Contains("statue") || combined.Contains("necropolis"))
                return CostTier.Low;

            // High cost indicators
            if (combined.Contains("gallery") || combined.Contains("palace") ||
                combined.Contains("citadel"))
                return CostTier.High;

            return CostTier.Medium;
        }

        private WeatherPref DetermineIndoorOutdoor(string tags)
        {
            var tagsLower = tags.ToLower();

            bool hasIndoor = tagsLower.Contains("indoor");
            bool hasOutdoor = tagsLower.Contains("outdoor");

            // If both or neither mentioned, default to Outdoor
            if (hasIndoor && !hasOutdoor)
                return WeatherPref.Indoor;

            return WeatherPref.Outdoor;
        }

        private SlotType? DetermineBestTimeOfDay(string tags)
        {
            var tagsLower = tags.ToLower();

            if (tagsLower.Contains("evening") || tagsLower.Contains("night"))
                return SlotType.Evening;

            if (tagsLower.Contains("morning"))
                return SlotType.Morning;

            if (tagsLower.Contains("afternoon"))
                return SlotType.Afternoon;

            // Most attractions are best in morning/afternoon
            return SlotType.Morning;
        }

        private int? DetermineAvgVisitDuration(string? category, string tags)
        {
            var categoryLower = category?.ToLower() ?? "";
            var tagsLower = tags.ToLower();

            // Escape rooms typically take 60-90 minutes
            if (categoryLower.Contains("escape") || tagsLower.Contains("escape"))
                return 90;

            // Museums and galleries typically 2-3 hours
            if (categoryLower.Contains("museum") || categoryLower.Contains("gallery"))
                return 150;

            // Parks and outdoor spaces
            if (categoryLower.Contains("park") || tagsLower.Contains("park"))
                return 120;

            // Default for attractions
            return 120;
        }

        private List<TravelVibe> ExtractVibesFromTags(string tags)
        {
            var vibes = new List<TravelVibe>();
            var tagsLower = tags.ToLower();

            if (tagsLower.Contains("family"))
                vibes.Add(TravelVibe.Family);

            if (tagsLower.Contains("photography"))
                vibes.Add(TravelVibe.Photography);

            if (tagsLower.Contains("romantic") || tagsLower.Contains("romance"))
                vibes.Add(TravelVibe.Romantic);

            if (tagsLower.Contains("calm") || tagsLower.Contains("peaceful"))
                vibes.Add(TravelVibe.Calm);

            if (tagsLower.Contains("adventure") || tagsLower.Contains("active"))
                vibes.Add(TravelVibe.Adventure);

            if (tagsLower.Contains("foodie") || tagsLower.Contains("food"))
                vibes.Add(TravelVibe.Foodie);

            // Default to Family if no specific vibe found
            if (!vibes.Any())
                vibes.Add(TravelVibe.Family);

            return vibes;
        }

        private List<ActivityKind> DetermineActivityTypes(string? category, string tags)
        {
            var activities = new List<ActivityKind>();
            var categoryLower = category?.ToLower() ?? "";
            var tagsLower = tags.ToLower();

            // Entertainment (escape rooms, attractions)
            if (categoryLower.Contains("attraction") || categoryLower.Contains("escape") ||
                tagsLower.Contains("fun") || tagsLower.Contains("entertainment"))
            {
                activities.Add(ActivityKind.Entertainment);
            }

            // Cultural (museums, galleries, historical sites)
            if (categoryLower.Contains("museum") || categoryLower.Contains("gallery") ||
                tagsLower.Contains("cultural") || tagsLower.Contains("historic") ||
                categoryLower.Contains("palace") || categoryLower.Contains("citadel"))
            {
                activities.Add(ActivityKind.Cultural);
            }

            // Shopping
            if (categoryLower.Contains("shopping") || categoryLower.Contains("mall") ||
                tagsLower.Contains("shopping") || categoryLower.Contains("market"))
            {
                activities.Add(ActivityKind.Shopping);
            }

            // Nature (parks, outdoor spaces)
            if (categoryLower.Contains("park") || tagsLower.Contains("outdoor") ||
                tagsLower.Contains("nature"))
            {
                activities.Add(ActivityKind.Nature);
            }

            // Dining
            if (categoryLower.Contains("restaurant") || categoryLower.Contains("cafe") ||
                tagsLower.Contains("food") || tagsLower.Contains("dining"))
            {
                activities.Add(ActivityKind.Dining);
            }

            // Default to Entertainment if nothing matched
            if (!activities.Any())
                activities.Add(ActivityKind.Entertainment);

            return activities;
        }

        private string GetActivityTypeDescription(ActivityKind kind)
        {
            return kind switch
            {
                ActivityKind.Dining => "Restaurants, cafes, and culinary experiences",
                ActivityKind.Shopping => "Markets, malls, and shopping districts",
                ActivityKind.Cultural => "Museums, galleries, and historical sites",
                ActivityKind.Nature => "Parks, gardens, and outdoor spaces",
                ActivityKind.Entertainment => "Attractions, entertainment venues, and activities",
                _ => $"Activities related to {kind}"
            };
        }
    }
}