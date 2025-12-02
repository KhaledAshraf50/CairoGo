namespace CairoGo.Models.Responsemodel
{
    public class ModelPerformanceStats
    {
        public string ModelName { get; set; } = string.Empty;
        public string ModelType { get; set; } = string.Empty;
        public float? AverageCTR { get; set; }
        public float? AverageConversion { get; set; }
        public int TotalPredictions { get; set; }
        public DateTime DeployedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
