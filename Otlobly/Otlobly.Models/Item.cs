using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Otlobly.Models
{
    public class Item
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }

        [Display(Name = "Coupon_Picture")]
        [DefaultValue("defult.png")]
        [RegularExpression(@"\.(jpg|jpeg|png|gif)$", ErrorMessage = "Invalid file format. Please upload a valid image.")]
        public byte[] Image { get; set; }
        [Required]
        public double Price { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public SubCategory SubCategory { get; set; }
        public int SubcategoryId { get; set; }
    }
}
