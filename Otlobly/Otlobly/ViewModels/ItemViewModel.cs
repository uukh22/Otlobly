using Otlobly.Models;

namespace Otlobly.ViewModels
{
    public class ItemViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? CategoryTitle { get; set; }
        public string? SubCategoryTitle { get; set; }
        public double price { get; set; }
        public byte[]? Image { get; set; }
        public int CategoryId { get; set; }
        public int SubcategoryId { get; set; }
        public List<Item>? Items { get; set; }
        public List<Category>? Categories { get; set; }
        public List<SubCategory>? SubCategories { get; set; }
        public List<Coupon>? Coupons { get; set; }

    }
}
