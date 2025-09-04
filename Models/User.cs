using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models
{
    public class User
    {
        [Key]
        public int UID { get; set; }

        [Required]
        public string UName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Invalid email format (e.g. example@gmail.com).")]
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        [Required]
        public string Password { get; set; } = string.Empty; // hashed password only

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Customer";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        [JsonIgnore]
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        [JsonIgnore]
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
