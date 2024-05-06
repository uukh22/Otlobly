using Microsoft.AspNetCore.Mvc;
using Otlobly.Repository;
using System.Security.Claims;

namespace Otlobly.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly OtloblyContext _context;
        public CartViewComponent(OtloblyContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);    
            if (claims != null) 
            {
                if (HttpContext.Session.GetInt32("SessionCart") != null)
                {
                    return View(HttpContext.Session.GetInt32("SessionCart"));
                }
                else
                {
                    HttpContext.Session.SetInt32("SessionCart", _context
                        .Carts.Where(x => x.User_Id == claims.Value).ToList().Count);

                    return View(HttpContext.Session.GetInt32("SessionCart"));
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
