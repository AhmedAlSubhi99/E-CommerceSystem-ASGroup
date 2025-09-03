using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    public class ReviewDTO
    {
        [Range(0, 5, ErrorMessage = "The value must be between 0 and 5.")]
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public int UID { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

    }

    public class ReviewCreateDTO
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
