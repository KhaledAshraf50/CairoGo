using System.ComponentModel.DataAnnotations;

namespace CairoGo.DTOs.TrendingTagDtos
{
    public class UpdateTagScoreDto
    {

        [Required(ErrorMessage = "Score is required")]
        [Range(0, float.MaxValue, ErrorMessage = "Score must be a positive number")]
        public float Score { get; set; }
    }
}
