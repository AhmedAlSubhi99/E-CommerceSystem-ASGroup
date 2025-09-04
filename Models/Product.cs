using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models
{
    public class Product
    {
        [Key]
        public int PID { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal OverallRating { get; set; } = 0; // default = 0 until reviews added

        public string? ImageUrl { get; set; }

        // ---------------------------
        // Navigation
        // ---------------------------
        [JsonIgnore]
        public virtual ICollection<OrderProducts> OrderProducts { get; set; } = new List<OrderProducts>();

        [JsonIgnore]
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        // ---------------------------
        // Foreign Keys
        // ---------------------------
        [Required]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Required]
        public int SupplierId { get; set; }
        public virtual Supplier? Supplier { get; set; }

        // ---------------------------
        // Concurrency
        // ---------------------------
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
