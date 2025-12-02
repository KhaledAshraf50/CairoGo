using System.ComponentModel.DataAnnotations;

namespace CairoGo.DTOs.ReviewsDtos
{
    public class ReviewDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Place ID is required")]
        public Guid PlaceId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public float Rating { get; set; }

        [MaxLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
        public string? Comment { get; set; }

        public string? PhotosJson { get; set; }

        public DateTime? VisitDate { get; set; }
    }
}
