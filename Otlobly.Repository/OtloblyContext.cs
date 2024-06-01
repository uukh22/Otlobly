using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Otlobly.Models;

namespace Otlobly.Repository
{
    public class OtloblyContext : IdentityDbContext<ApplicationUser>
    {
        public OtloblyContext(DbContextOptions<OtloblyContext> options)
            : base(options) { }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Carts)
                .WithOne(c => c.User)
                .HasForeignKey(o => o.ApplicationUserId);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.ApplicationUserId);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Item)
                .WithMany()
                .HasForeignKey(c => c.Item_Id);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.SubCategories)
                .WithOne(s => s.Category)
                .HasForeignKey(s => s.Category_Id);


            modelBuilder.Entity<OrderHeader>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.OrderHeader)
                .HasForeignKey(od => od.OrderHeaderID);

            modelBuilder.Entity<OrderHeader>()
                .HasOne(o => o.Coupon) 
                .WithMany()
                .HasForeignKey(o => o.CouponId);
        }
    }

}