﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Otlobly.Models;
using Otlobly.Repository;
using Otlobly.Utility;
using Otlobly.ViewModels;
using Stripe.Checkout;
using System.Security.Claims;
using Session = Stripe.Checkout.Session;
using SessionCreateOptions = Stripe.Checkout.SessionCreateOptions;
using SessionService = Stripe.Checkout.SessionService;

namespace Otlobly.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer, Admin, Manager")]
    public class CartController : Controller
    {
        private readonly OtloblyContext _context;

        [BindProperty]
        public CardOrderViewModel details { get; set; }

        public CartController(OtloblyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            details = new CardOrderViewModel()
            {
                OrderHeader = new OrderHeader(),
            };

            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            details.ListOfCard = _context.Carts.Where(x => x.ApplicationUserId == claim.Value).ToList();

            if (details.ListOfCard != null)
            {
                foreach (var item in details.ListOfCard)
                {
                    var items = _context.Items.FirstOrDefault(x => x.Id == item.Item_Id);
                    if (items != null)
                    {
                        details.OrderHeader.OrderTotal += items.Price * item.count;
                        item.Item = items;
                    }
                }
            }
            return View(details);
        }

        [HttpGet]
        public async Task<IActionResult> Summary()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                return Unauthorized("User is not logged in.");
            }

            // Check if there's an existing order that's not completed
            var existingOrder = await _context.OrderHeaders
                .FirstOrDefaultAsync(o => o.ApplicationUserId == claim.Value &&
                                          o.PaymentStatus == PaymentStatus.StatusPending);

            if (existingOrder != null)
            {
                // If an order exists and is pending, redirect to the Stripe session or handle accordingly
                return Redirect(existingOrder.SessionUrl); // Assuming SessionUrl is stored
            }

            var details = new CardOrderViewModel
            {
                ListOfCard = await _context.Carts.Include(x => x.Item).Where(x => x.ApplicationUserId == claim.Value).ToListAsync(),
                OrderHeader = new OrderHeader
                {
                    OrderStatus = OrderStatus.StatusPending,
                    PaymentStatus = PaymentStatus.StatusPending,
                    ApplicationUserId = claim.Value,
                    TimeOfPick = DateTime.Now
                }
            };

            if (details.ListOfCard == null || details.ListOfCard.Count == 0)
            {
                return NotFound("No cart items found.");
            }

            details.OrderHeader.ApplicationUser = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == claim.Value);
            if (details.OrderHeader.ApplicationUser == null)
            {
                return NotFound("User not found.");
            }

            details.OrderHeader.Name = details.OrderHeader.ApplicationUser.Name;
            details.OrderHeader.Phone = details.OrderHeader.ApplicationUser.PhoneNumber;

            foreach (var cart in details.ListOfCard)
            {
                details.OrderHeader.OrderTotal += (cart.Item.Price * cart.count);
            }

            await _context.OrderHeaders.AddAsync(details.OrderHeader);
            await _context.SaveChangesAsync();

            var domain = "https://localhost:5168/";
            var options = new SessionCreateOptions
            {
                LineItems = details.ListOfCard.Select(cart => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(cart.Item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = cart.Item.Title
                        },
                    },
                    Quantity = cart.count,
                }).ToList(),
                Mode = "payment",
                SuccessUrl = domain + $"Customer/Cart/OrderSuccess?id={details.OrderHeader.Id}",
                CancelUrl = domain + $"Customer/Cart/Index"
            };

            var services = new SessionService();
            Session session = await services.CreateAsync(options);
            var orderHeader = await _context.OrderHeaders.FirstOrDefaultAsync(x => x.Id == details.OrderHeader.Id);
            if (orderHeader != null)
            {
                orderHeader.DateOfPick = DateTime.Now;
                orderHeader.Trans_Id = session.PaymentIntentId;
                orderHeader.SessionId = session.Id;
                await _context.SaveChangesAsync();
            }

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303); // Redirection to Stripe Checkout
        }

        [HttpGet]
        public async Task<IActionResult> OrderSuccess(int id)
        {
            
            var orderHeader = await _context.OrderHeaders.FirstOrDefaultAsync(x => x.Id == id);

            if (orderHeader == null)
            {
                return NotFound("Order not found.");
            }



            var service = new SessionService();
            Session session = await service.GetAsync(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                orderHeader.Trans_Id = session.PaymentIntentId;
                orderHeader.OrderStatus = orderStatus.StatusApproved;
                orderHeader.PaymentStatus = PaymentStatus.StatusApproved;
                orderHeader.OrderDate = DateTime.Now;

                var carts = await _context.Carts.Where(x => x.ApplicationUserId == orderHeader.ApplicationUserId).ToListAsync();
                _context.Carts.RemoveRange(carts);
                _context.SaveChanges();

                //    _context.Entry(orderHeader).State = EntityState.Modified;
                _context.OrderHeaders.Update(orderHeader);
                await _context.SaveChangesAsync();
                var count = _context.Carts.Count(x => x.ApplicationUserId == orderHeader.ApplicationUserId);

                HttpContext.Session.SetInt32("Session", count);

                return RedirectToAction("Index", "Home");
            }

            // Handle cases where payment is not confirmed
            return View("PaymentFailed"); // Or whatever view you use to show payment failure
        }

        public async Task<IActionResult> Plus(int Id)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == Id);
            cart.count += 1;
            // _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int Id)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == Id);
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            cart.ApplicationUserId = claim.Value;
            if (cart.count == 1)
            {
                _context.Carts.Remove(cart);
                _context.SaveChanges();
            }
            else
            {
                cart.count -= 1;

                 _context.SaveChanges();
                var count =  _context.Carts.Count(x => x.ApplicationUserId == claim.Value);
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

            var count = _context.Carts.Count(x => x.ApplicationUserId == claim.Value);
            HttpContext.Session.SetInt32("Session", count);
            return RedirectToAction(nameof(Index));
        }

    }
}
