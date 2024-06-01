using System.ComponentModel.DataAnnotations;

namespace Otlobly.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public ICollection <Item> Items { get; set; }
        public ICollection <SubCategory> SubCategories { get; set; }
    }
}
