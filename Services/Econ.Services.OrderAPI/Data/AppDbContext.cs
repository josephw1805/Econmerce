using Microsoft.EntityFrameworkCore;

namespace Econ.Services.OrderAPI;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<OrderHeader> OrderHeaders { get; set; }
  public DbSet<OrderDetails> OrderDetails { get; set; }
}
