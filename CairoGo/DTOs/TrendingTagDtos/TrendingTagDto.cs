using System.ComponentModel.DataAnnotations;

namespace CairoGo.DTOs.TrendingTagDtos
{
    public class TrendingTagDto
    {
        [Required(ErrorMessage = "Tag name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tag name must be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Range(0, float.MaxValue, ErrorMessage = "Score must be a positive number")]
        public float Score { get; set; }

        [Required(ErrorMessage = "Period start date is required")]
        [DataType(DataType.DateTime)]
        public DateTime PeriodStart { get; set; }

        [Required(ErrorMessage = "Period end date is required")]
        [DataType(DataType.DateTime)]
        public DateTime PeriodEnd { get; set; }
    }
}
