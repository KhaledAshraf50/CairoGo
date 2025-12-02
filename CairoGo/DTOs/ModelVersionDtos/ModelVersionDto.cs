using System.ComponentModel.DataAnnotations;

namespace CairoGo.DTOs.ModelVersionDtos
{
    public class ModelVersionDto
    {
        [Required(ErrorMessage = "Model name is required")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model type is required")]
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty;

        public string? ConfigJson { get; set; }

        public bool IsActive { get; set; } = false;
    }
}
