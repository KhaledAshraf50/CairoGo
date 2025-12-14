namespace CairoGo.DTOs.MLIntegration
{
    public class MLRecommendationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public float Final_Score { get; set; } // Changed from Score to Final_Score
    }
}
