using CairoGo.Models.ENums;
using System.ComponentModel.DataAnnotations;

namespace CairoGo.DTOs.InteractionDtos
{
    public class InteractionDto
    {
            [Required(ErrorMessage = "User ID is required")]
            public Guid UserId { get; set; }

            [Required(ErrorMessage = "Place ID is required")]
            public Guid PlaceId { get; set; }

            [Required(ErrorMessage = "Interaction type is required")]
            public InteractionType Type { get; set; }

            public string? ContextJson { get; set; }
    }
    
}
