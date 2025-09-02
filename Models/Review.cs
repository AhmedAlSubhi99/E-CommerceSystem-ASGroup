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
        [Range(1,5)]
        public int Rating { get; set; }

        public string Comment { get; set; } = null;

        public DateTime ReviewDate { get; set; }

        [ForeignKey("user")]
        public int UID { get; set; }

        [JsonIgnore]
        public User user { get; set; }

        [ForeignKey("product")]
        public int PID { get; set; }

        [JsonIgnore]
        public Product product { get; set; }
    }
}
