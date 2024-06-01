using Otlobly.Models;

namespace Otlobly.ViewModels
{
    public class CardOrderViewModel
    {
        public List<Cart> ListOfCard {  get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
