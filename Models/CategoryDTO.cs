using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    public  class CategoryCreateDTO
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
    public class CategoryUpdateDto : CategoryCreateDTO { }
    public class CategoryDTO
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
    

}
