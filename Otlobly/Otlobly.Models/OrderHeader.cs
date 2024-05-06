using System.ComponentModel.DataAnnotations;

namespace Otlobly.Models
{
    public class OrderHeader
    {
        public readonly string SessionId;
        [Key]
        public int Id { get; set; }
        public string User_Id { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        public DateTime TimeOfPick { get; set; }
        public DateTime DateOfPick { get; set; }
        public double SubTotal { get; set; }
        public double OrderTotal { get; set; }
        public string CouponCode { get; set; }
        public double CouponDis { get; set; }
        public string Trans_Id { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int? CouponId { get; set; }
        public Coupon Coupon { get; set; }
        public ICollection<Cart> Payments { get; set; }
        public ICollection<OrderDetails> OrderDetails { get; set; }
    }
}
