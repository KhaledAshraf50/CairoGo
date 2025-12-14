namespace CairoGo.DTOs.MLIntegration
{
    public class MLItineraryResponseDto
    {
        // Dictionary with keys like "Plan 1", "Plan 2", etc.
        public Dictionary<string, List<MLPlanPlaceDto>> Plans { get; set; } = new();
    }
    public class MLPlanPlaceDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public float? Rating { get; set; }
        public float Final_Score { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
    }
}
