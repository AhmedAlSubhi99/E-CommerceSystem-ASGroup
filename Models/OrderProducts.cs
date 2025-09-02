using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models
{
    [PrimaryKey(nameof(OID), nameof(PID))]
    public class OrderProducts
    {

        [Range (0, int.MaxValue)]
        public int Quantity { get; set; }

        
        [ForeignKey("Order")]

        public int OID { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }

        
        [ForeignKey("Product")]
        public int PID { get; set; }
        [JsonIgnore]
        public Product product { get; set; }
    }
}

