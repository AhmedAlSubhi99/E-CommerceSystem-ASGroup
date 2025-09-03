using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models
{
    public class Order
    {
        [Key]
        public int OID { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [ForeignKey("user")]
        public int UID { get; set; }
        public virtual User user { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<OrderProducts> OrderProducts { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? StatusUpdatedAtUtc { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
