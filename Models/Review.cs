using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models
{
    public class Review
    {
        [Key]
        public int ReviewID { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        [Required]
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        // ---------------------------
        // Foreign Keys
        // ---------------------------
        [Required]
        [ForeignKey(nameof(User))]
        public int UID { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(Product))]
        public int PID { get; set; }

        [JsonIgnore]
        public virtual Product Product { get; set; } = null!;
    }
}
