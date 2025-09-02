using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    public class ReviewDTO
    {
        [Range(0, 5, ErrorMessage = "The value must be between 0 and 5.")]
        public int Rating { get; set; }

        public string Comment { get; set; } = null;

    }
}
