using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Otlobly.Models;
using Otlobly.Repository;
using Otlobly.Utility;
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
            builder.Services.Configure<PaymentStatus>(builder.Configuration.GetSection("Stripe"));    

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<OtloblyContext>()
               .AddDefaultTokenProviders();

            builder.Services.AddScoped<IDbInitializer, DbInitializer>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddSession();
            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            var serviceProvider = builder.Services.BuildServiceProvider();
            var stripeSettings = serviceProvider.GetRequiredService<IOptionsSnapshot<PaymentStatus>>().Value;
            StripeConfiguration.ApiKey = stripeSettings.Secretkey;

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
            app.UseSession();

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
