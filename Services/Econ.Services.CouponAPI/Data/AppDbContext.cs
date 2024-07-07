using Microsoft.EntityFrameworkCore;

namespace Econ.Services.CouponAPI;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Coupon> Coupons { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Coupon>().HasData(new Coupon
    {
      CouponId = 1,
      CouponCode = "10OFF",
      DiscountAmount = 10,
      MinAmount = 20
    });

    modelBuilder.Entity<Coupon>().HasData(new Coupon
    {
      CouponId = 2,
      CouponCode = "20OFF",
      DiscountAmount = 20,
      MinAmount = 40
    });
  }
}
