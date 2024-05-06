using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Otlobly.Models;
using Otlobly.Repository;
using Otlobly.Utility;
using Otlobly.ViewModels;
using Stripe;
using System.Security.Claims;

namespace Otlobly.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]

    public class OrderController : Controller
    {
        private readonly OtloblyContext _context;
        public OrderController(OtloblyContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index(string status)
        {
            IEnumerable<OrderHeader> order;
            if (User.IsInRole("Admin")|| User.IsInRole("Manager"))
            {
                order = _context.OrderHeaders.Include(x => x.ApplicationUser).ToList();

            }
            else
            {
                    var climsIdentity = (ClaimsIdentity)User.Identity;
                    var clims = climsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                    order = _context.OrderHeaders.Where(x => x.User_Id == clims.Value);
            }
            switch (status)
            {
                case "Pending":
                    order = order.Where(x => x.PaymentStatus == PaymentStatus.StatusPending); 
                    break;
                case "Approved":
                    order = order.Where(x => x.PaymentStatus == PaymentStatus.StatusApproved); 
                    break;
                case "UnderProcessing":
                    order = order.Where(x => x.OrderStatus == orderStatus.StatusUnderProcessing); 
                    break;
                case "Shipped":
                    order = order.Where(x => x.OrderStatus == orderStatus.StatusPending); 
                    break;
                default: 
                    break;
            }
            return View(order);
        }
        public IActionResult OrderDetils(int Id)
        {
            var OrderDetail = new OrderDetilsViewModel()
            {
                orderHeader = _context.OrderHeaders.Include(x => x.ApplicationUser)
                .FirstOrDefault(x => x.Id == Id),
                
                orderDetails = _context.OrderDetails.Include(x => x.Item)
                .Where(item => item.Id == Id).ToList()

            };
            return View(OrderDetail);
        }
        public IActionResult InPorcess(OrderDetilsViewModel orderDetilsViewModel)
        {
            var orderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == orderDetilsViewModel.orderHeader.Id);
            orderHeader.OrderStatus = orderStatus.StatusUnderProcessing;
            _context.SaveChanges();
            TempData["succes"] = "Order Status Updated-Inporcess";
            return RedirectToAction("OrderDetails", "Orders", new { id = orderDetilsViewModel.orderHeader.Id });
        }
        public IActionResult Shipped(OrderDetilsViewModel orderDetilsViewModel)
        {
            var orderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == orderDetilsViewModel.orderHeader.Id);
            orderHeader.OrderStatus = orderStatus.StatusShpped;
            _context.OrderHeaders.Update(orderHeader);
            _context.SaveChanges();
            TempData["success"] = "Order Status Updated-Shipped";
            return RedirectToAction("OrderDetails", "Orders", new { id = orderDetilsViewModel.orderHeader.Id });
        }
        public IActionResult CalncelOrder(OrderDetilsViewModel orderDetilsViewModel)
        {
            var OrderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == orderDetilsViewModel.orderHeader.Id);
            if (OrderHeader.PaymentStatus == PaymentStatus.StatusApproved)
            {
                var refund = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = OrderHeader.Trans_Id
                };
                var Service = new RefundService();
                Stripe.Refund refund1 = Service.Create(refund);
                OrderHeader.OrderStatus = orderStatus.StatusCancelled;
                OrderHeader.PaymentStatus = PaymentStatus.StatusRejected;
            }
            else
            {
                OrderHeader.OrderStatus = orderStatus.StatusCancelled;
                OrderHeader.PaymentStatus = PaymentStatus.StatusRejected;
            }
            _context.SaveChanges();
            TempData["Succes"] = "Order Cancelld";
            return RedirectToAction("OrderDetails", "Orders", new { id = orderDetilsViewModel.orderHeader.Id });

        }
    }
}
