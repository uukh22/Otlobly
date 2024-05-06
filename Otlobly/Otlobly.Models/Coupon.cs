using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Otlobly.Models
{
    public class Coupon
    {
        [Key]
        public int Coupon_Id { get; set; }
        [Required]
        public string Coupon_Name { get; set; }
        public string Title { get; set; }
        [Required]
        public string Coupon_Type { get; set; }
        [Required]
        public double Discount { get; set; }
        [Required]
        public double MinimumAmount { get; set; }

        [Display(Name = "Coupon_Picture")]
        [DefaultValue("defult.png")]
       // [Column(TypeName = "varbinary(max)")]
        [RegularExpression(@"\.(jpg|jpeg|png|gif)$", ErrorMessage = "Invalid file format. Please upload a valid image.")]

        public byte[] Coupon_Picture { get; set; }
        [Required]
        public bool IsActive { get; set; }
    }
    public enum CouponType
    {
        persent = 0,
        currency = 1
    }
}