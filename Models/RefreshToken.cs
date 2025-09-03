using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime Expires { get; set; }

        public DateTime? Revoked { get; set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => Revoked == null && !IsExpired;

        // Relation to User
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
    }
}