using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models.DTO
{
    // ==================== For API responses ====================
    public class ReviewDTO
    {
        public int ReviewID { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime ReviewDate { get; set; }

        // Extra info for clients
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int ProductId { get; set; }
    }

    // ==================== For creating reviews ====================
    public class ReviewCreateDTO
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    // ==================== For updating reviews ====================
    public class ReviewUpdateDTO
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}
