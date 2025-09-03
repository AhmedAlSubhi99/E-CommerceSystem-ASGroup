using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    public class LoginDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
