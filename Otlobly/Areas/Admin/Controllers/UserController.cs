using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Otlobly.Models;
using Otlobly.Repository;
using System.Security.Claims;
using Otlobly.ViewModels;


namespace Otlobly.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin , Manager")]
    public class UserController : Controller
    {
        private readonly OtloblyContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(OtloblyContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var users = _context.Users.Where(x => x.Id != claim.Value).ToList();

            var model = new List<UserViewModel>();

            foreach (var user in users)
            {
                var userRoles = _userManager.GetRolesAsync(user).Result;
                var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;

                model.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = string.Join(", ", userRoles),
                    IsLocked = isLocked
                });
            }

            return View(model);
        }
        
        public async Task<IActionResult> Lock(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("User not found.");
            }
            if ( await _userManager.IsInRoleAsync(user, "Admin"))
            {
                // Admins cannot be locked, and managers cannot lock admins
                return Forbid();
            }

            user.LockoutEnd = DateTimeOffset.MaxValue; 

            await _context.SaveChangesAsync();

            return Ok($"User with ID {id} has been locked.");
        }

        //[Authorize(Roles ="Admin")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddManager(ApplicationUser user)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Users.Add(user);

        //        await _userManager.AddToRoleAsync(user, "Manager");

        //        await _context.SaveChangesAsync();

        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(user);
        //}

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoleToManager(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Remove existing roles and add Manager role
            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);
            await _userManager.AddToRoleAsync(user, "Manager");

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoleToCustomer(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Remove existing roles and add Customer role
            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);
            await _userManager.AddToRoleAsync(user, "Customer");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.LockoutEnd = null;

            await _context.SaveChangesAsync();

            return Ok($"User with ID {id} has been unlocked.");
        }

    }

  
}
