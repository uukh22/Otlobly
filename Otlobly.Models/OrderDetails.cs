
using System.ComponentModel.DataAnnotations;

namespace Otlobly.Models
{
    public class OrderDetails
    {
        [Key]
        public int Id { get; set; }
        public int OrderHeaderID { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public int ItemID { get; set; }
        public Item Item { get; set; }
        public int count { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
    }
}
