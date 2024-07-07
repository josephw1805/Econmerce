using Microsoft.EntityFrameworkCore;

namespace Econ.Services.ShoppingCartAPI;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<CartHeader> CartHeaders { get; set; }
  public DbSet<CartDetails> CartDetails { get; set; }
}
