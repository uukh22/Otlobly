using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Otlobly.Models;
using Otlobly.Repository;
using Otlobly.Utility;
using Otlobly.ViewModels;
using Stripe;
using Stripe.BillingPortal;
using Stripe.Checkout;
using System.Security.Claims;
using Session = Stripe.Checkout.Session;
using SessionCreateOptions = Stripe.Checkout.SessionCreateOptions;
using SessionService = Stripe.Checkout.SessionService;


namespace Otlobly.Areas.Customer.Controllers
{
    [Area("Customer")]
   // [Authorize(Roles ="Customer")]
    public class CartController : Controller
    {
        private readonly OtloblyContext _context;

        [BindProperty]
        public CardOrderViewModel details { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CartController(OtloblyContext context)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _context = context;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            details = new CardOrderViewModel()
            {
                OrderHeader = new OrderHeader()
            };
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            details.ListOfCard = _context.Carts.Where(x => x.User_Id == claim.Value).ToList();

            if (details.ListOfCard != null)
            {
                foreach (var item in details.ListOfCard)
                {
                    details.OrderHeader.OrderTotal += (item.Item.Price * item.count);
                }
            }
            return View(details);
        }

        [HttpGet]
        public async Task<IActionResult> Summary(CardOrderViewModel cartOrderViewModel)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            details.ListOfCard = _context.Carts.Where(x => x.User_Id == claim.Value).Include("item").ToList();
            details.OrderHeader.OrderStatus = orderStatus.StatusPending;
            details.OrderHeader.PaymentStatus = PaymentStatus.StatusPending;

            details.OrderHeader.User_Id = claim.Value;

            details = new CardOrderViewModel()
            {
                ListOfCard = _context.Carts.Include(x => x.Item).Where(y => y.User_Id == claim.Value).ToList(),
                OrderHeader = new OrderHeader()
            };
            details.OrderHeader.ApplicationUser = _context.ApplicationUsers.Where(x => x.Id == claim.Value).FirstOrDefault();
            details.OrderHeader.Name = details.OrderHeader.ApplicationUser.Name;
            details.OrderHeader.Phone = details.OrderHeader.ApplicationUser.PhoneNumber;
            details.OrderHeader.TimeOfPick = DateTime.Now;

            foreach (var cart in details.ListOfCard)
            {
                details.OrderHeader.OrderTotal += (cart.Item.Price * cart.count);
            }
            
            _context.OrderHeaders.Add(details.OrderHeader);
            _context.SaveChanges();

            foreach (var cart in details.ListOfCard)
            {
                OrderDetails orderDetails = new OrderDetails()
                {
                    ItemID = cart.Item_Id,
                    OrderHeaderID = details.OrderHeader.Id,
                    count = cart.count,
                    Price = cart.Item.Price
                };
                _context.Carts.RemoveRange(details.ListOfCard);
                _context.SaveChanges();
            }

            var count = _context.Carts.Where(x => x.User_Id == claim.Value).ToList().Count();
            HttpContext.Session.SetInt32("SessionCart", count);

            var domain = "https://localhost:44350/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"customer/cart/ordersuccess?id={details.OrderHeader.Id}",
                CancelUrl = domain + $"customer/cart/Index"
            };

            foreach(var cart in details.ListOfCard)
            {
                var listItemOptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(cart.Item.Price * 100),
                        Currency = "INR",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = cart.Item.Title
                        },
                    },
                    Quantity = cart.count,
                };
                options.LineItems.Add(listItemOptions);
            }

            var services = new SessionService();
            Session session = services.Create(options);
            var orderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id.ToString() == session.Id);
            orderHeader.DateOfPick = DateTime.Now;
            orderHeader.Trans_Id = session.PaymentIntentId;
            orderHeader.User_Id = session.Id;
            _context.SaveChanges();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }
        [HttpPost]
        public IActionResult Summary(string StipeToken)
        {
            return RedirectToAction("Index");

        }
        public IActionResult ordersuccess(int Id)
        {
            var Orderheader = _context.OrderHeaders.Where(x => x.Id == Id).FirstOrDefault();
            var service = new SessionService();
            Session session = service.Get(Orderheader.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                //Orderheader.PaymentIntentId = session.PaymentIntentId;
                Orderheader.OrderStatus = orderStatus.StatusApproved;
                Orderheader.PaymentStatus = PaymentStatus.StatusApproved;
            }
            List<Cart> cart = _context.Carts.Where(x => x.User_Id == Orderheader.User_Id).ToList();
            foreach (var item in cart)
            {
                _context.Carts.Remove(item);
            }
            _context.SaveChanges();

            return View(Id);
        }
        public async Task<IActionResult> Plus(int Id)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == Id);
            cart.count += 1;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Minus(int Id)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == Id);
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            cart.User_Id = claim.Value;
            if (cart.count == 1)
            {
                _context.Carts.Remove(cart);
                _context.SaveChanges();
            }
            else
            {
                cart.count -= 1;

                await _context.SaveChangesAsync();
                var count = _context.Carts.Where(x => x.User_Id == claim.Value).ToList().Count();
                HttpContext.Session.SetInt32("Session", count);

            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int Id)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == Id);

            _context.Carts.Remove(cart);

            _context.SaveChanges();

            var count = _context.Carts.Where(x => x.User_Id == claim.Value).ToList().Count();
            HttpContext.Session.SetInt32("Session", count);
            return RedirectToAction(nameof(Index));
        }
     

    }
}
