using System.ComponentModel.DataAnnotations;

namespace Otlobly.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        public int Item_Id { get; set; }
        public Item Item{ get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Count must be at least 1")]
        public int count { get; set; }
    }
}
