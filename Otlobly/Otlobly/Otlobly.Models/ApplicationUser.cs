using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Otlobly.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        [Display(Name = "Image")]
        [DefaultValue("defult.png")]
      //  [Required(ErrorMessage = "Please select a profile picture .jpg , .jpeg , .png,  .gif ")]
       // [RegularExpression(@"\.(jpg||jpeg||png||gif)$", ErrorMessage = "Invalid file format. Please upload a valid image.")]
        public string ProfilePicture { get; set; }
        public ICollection<OrderHeader> OrderHeaders { get; set; }
        public ICollection<Cart> Carts { get; set; }
    }
}
