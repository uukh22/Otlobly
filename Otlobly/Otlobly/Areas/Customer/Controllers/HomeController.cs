using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Otlobly.Models;
using Otlobly.Repository;
using Otlobly.ViewModels;
using System.Security.Claims;


namespace Otlobly.Areas.Customer.Controllers
{
    [Area("Customer")]
    
    public class HomeController : Controller
    {
        private readonly OtloblyContext _context;
        public HomeController(OtloblyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ItemViewModel vm = new ItemViewModel()
            {
                Items = await _context.Items.Include(x => x.Category).Include(y => y.SubCategory).ToListAsync(),
                Categories = await _context.Categories.ToListAsync(),
                Coupons = await _context.Coupons.Where(c => c.IsActive == true).ToListAsync()
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int Id)
        {
            var ItemFromDb = await _context.Items.Include(x => x.Category)
                .Include(y => y.SubCategory).Where(x => x.Id == Id).FirstOrDefaultAsync();

            var cart = new Cart()
            {
                Item = ItemFromDb,
                Item_Id = ItemFromDb.Id,
                count = 1
            };
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Details(Cart cart)
        {
            cart.Id = 0;
            if (ModelState.IsValid)
            {
                var claimIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cart.User_Id = claim.Value;

                var cartFromDb = await _context.Carts.Where(x => x.User_Id == claim.Value && x.Item_Id == cart.Item_Id).FirstOrDefaultAsync();

                if (cartFromDb == null)
                {
                    _context.Carts.Add(cart);
                }
                else
                {
                    cartFromDb.count += cart.count;
                }
                await _context.SaveChangesAsync();

                var count = await _context.Carts.Where(c => c.User_Id == claim.Value).CountAsync();
                HttpContext.Session.SetInt32("SessionCart", count);

                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}
