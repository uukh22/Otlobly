using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Otlobly.Models;
using Otlobly.Repository;
using Stripe;


namespace Otlobly
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            //var connectionString = builder.Configuration.GetConnectionString("OtloblyContextConnection") ?? throw new InvalidOperationException("Connection string 'OtloblyContextConnection' not found.");

            builder.Services.AddDbContext<OtloblyContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("OtloblyContextConnection"), b => b.MigrationsAssembly("Otlobly")));
            
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOrManagerPolicy", policy =>
                policy.RequireRole("Admin", "Manager"));
            });

            //builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<OtloblyContext>();

            //builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<OtloblyContext>();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<OtloblyContext>()
               .AddDefaultTokenProviders();

            builder.Services.AddScoped<IDbInitializer, DbInitializer>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            
            builder.Services.AddRazorPages();
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            DataSeeding();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapRazorPages();

            app.MapControllerRoute(
               name: "identity",
               pattern: "{area}/{controller=Account}/{action=Login}/{id?}");

            app.MapControllerRoute(
               name: "default",
               pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();

            void DataSeeding()
            {
                using (var scope = app.Services.CreateScope())
                {
                    var DbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                    DbInitializer.Initialize();
                }
            }
        }
    }
}
