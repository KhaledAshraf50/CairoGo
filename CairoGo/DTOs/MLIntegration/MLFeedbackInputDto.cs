namespace CairoGo.DTOs.MLIntegration
{
    public class MLFeedbackInputDto
    {
        public string ItemName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "click", "bookmark", "view"
    }
}
