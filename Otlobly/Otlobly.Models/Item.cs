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
        public string Title { get; set; }
        public string Description { get; set; }

        [Display(Name = "Coupon_Picture")]
        [DefaultValue("defult.png")]
     //   [Column(TypeName = "varbinary(max)")]
        [RegularExpression(@"\.(jpg|jpeg|png|gif)$", ErrorMessage = "Invalid file format. Please upload a valid image.")]

        public byte[] Image { get; set; }
        public double Price { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public SubCategory SubCategory { get; set; }
        public int SubcategoryId { get; set; }
    }
}
