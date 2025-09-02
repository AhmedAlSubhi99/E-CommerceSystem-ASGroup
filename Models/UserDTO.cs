using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models
{
    public class UserDTO
    {
       
        [Required]
        public string UName { get; set; }

        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        ErrorMessage = "Invalid email format.(e.g 'example@gmail.com')")]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,}$",
        ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter," +
            " one lowercase letter, one digit")]
        public string Password { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Role { get; set; }

    }
}
