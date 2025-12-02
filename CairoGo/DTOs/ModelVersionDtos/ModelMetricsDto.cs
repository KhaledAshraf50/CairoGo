using System.ComponentModel.DataAnnotations;

namespace CairoGo.DTOs.ModelVersionDtos
{
    public class ModelMetricsDto
    {
        [Range(0, 1, ErrorMessage = "CTR must be between 0 and 1")]
        public float AverageCTR { get; set; }

        [Range(0, 1, ErrorMessage = "Conversion must be between 0 and 1")]
        public float AverageConversion { get; set; }
    }
}
