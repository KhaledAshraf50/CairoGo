namespace CairoGo.DTOs.MLIntegration
{
    public class MLItineraryResponseDto
    {
        public Dictionary<string, List<MLDayPlaceDto>> Itinerary { get; set; } = new();

    }
    public class MLDayPlaceDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public float Rating { get; set; }
        public float Popularity_Score { get; set; }
    }
}
