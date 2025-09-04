using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models
{
    [PrimaryKey(nameof(OID), nameof(PID))]
    public class OrderProducts
    {
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        // Foreign key to Order
        [ForeignKey(nameof(Order))]
        public int OID { get; set; }
        [JsonIgnore]
        public Order Order { get; set; } = null!;

        // Foreign key to Product
        [ForeignKey(nameof(Product))]
        public int PID { get; set; }
        [JsonIgnore]
        public Product Product { get; set; } = null!;

        // Price at the time of order (snapshot)
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        // Calculated total
        [NotMapped]
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
