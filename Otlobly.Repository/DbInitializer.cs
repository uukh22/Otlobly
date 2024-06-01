using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Otlobly.Models;
using System;

namespace Otlobly.Repository
{
    public class DbInitializer : IDbInitializer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly OtloblyContext _context;

        public DbInitializer(RoleManager<IdentityRole> roleManager,
               UserManager<ApplicationUser> userManager,
               OtloblyContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public void Initialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception)
            {
                throw;
            }

            // Create roles if they don't exist
            string[] roles = { "Admin", "Manager", "Customer" };
            foreach (var role in roles)
            {
                if (!_roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                }
            }

            // Create the admin user if it doesn't exist
            if (_userManager.FindByEmailAsync("admin@gmail.com").GetAwaiter().GetResult() == null)
            {
                var user = new ApplicationUser()
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    Name = "Admin",
                    PhoneNumber = "12345678",
                    City = "Xyz",
                    ProfilePicture = ""
                };

                // Create the user with the specified password
                _userManager.CreateAsync(user, "Admin@123").GetAwaiter().GetResult();

                // Add the "Admin" role to the user
                _userManager.AddToRoleAsync(user, "Admin").GetAwaiter().GetResult();
            }
        }
    }
}
