﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Otlobly.Models;
using Otlobly.Repository;
using Otlobly.Utility;
using Otlobly.ViewModels;
using Stripe;
using Stripe.TestHelpers;
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
                    order = _context.OrderHeaders.Where(x => x.ApplicationUserId == clims.Value);
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
        public IActionResult InPorcess(int Id)
        {
            var orderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == Id);
            if (orderHeader.OrderStatus != orderStatus.StatusCancelled)
            {
                orderHeader.OrderStatus = orderStatus.StatusUnderProcessing;
            }
            _context.OrderHeaders.Update(orderHeader);
            _context.SaveChanges();
            
            TempData["succes"] = "Order Status Updated-Inporcess";

            return RedirectToAction("Index", "Order", new { id = Id });
        }
        public IActionResult Shipped(int Id)
        {
            var orderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == Id);
            if (orderHeader.OrderStatus != orderStatus.StatusCancelled)
            {
                orderHeader.OrderStatus = orderStatus.StatusShpped;
                orderHeader.DateOfPick = DateTime.Now;
            }
            _context.OrderHeaders.Update(orderHeader);
            _context.SaveChanges();
           
            TempData["success"] = "Order Status Updated-Shipped";
           
            return RedirectToAction("Index", "Order", new { id = Id });
        }
        public IActionResult CalncelOrder(int Id)
        {
            var OrderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == Id);
            if (OrderHeader.OrderStatus != orderStatus.StatusShpped)
            {
                if (OrderHeader.PaymentStatus == PaymentStatus.StatusApproved)
                {
                    var options = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = OrderHeader.Trans_Id
                    };
                    var Service = new Stripe.RefundService();

                    var refund = Service.Create(options);

                    OrderHeader.OrderStatus = orderStatus.StatusCancelled;
                    OrderHeader.PaymentStatus = PaymentStatus.StatusRejected;
                }
                else
                {
                    OrderHeader.OrderStatus = orderStatus.StatusCancelled;
                    OrderHeader.PaymentStatus = PaymentStatus.StatusRejected;
                }
            }
          
            
            _context.OrderHeaders.Update(OrderHeader);
            _context.SaveChanges();
            TempData["Succes"] = "Order Cancelld";
            return RedirectToAction("Index", "Order", new { id = Id });

        }
    }
}
