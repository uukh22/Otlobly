using Otlobly.Models;

namespace Otlobly.ViewModels
{
    public class OrderDetilsViewModel
    {
        public OrderHeader orderHeader {  get; set; }
        public IEnumerable<OrderDetails>orderDetails { get; set; }

    }
}
