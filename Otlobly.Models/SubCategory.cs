using System.ComponentModel.DataAnnotations;

namespace Otlobly.Models
{
    public class SubCategory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public int Category_Id { get; set; }
        public Category Category { get; set; }
        public ICollection<Item> Items { get; set; }    
    }
}
