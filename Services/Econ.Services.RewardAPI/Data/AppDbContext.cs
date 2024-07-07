using Microsoft.EntityFrameworkCore;

namespace Econ.Services.RewardAPI;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Rewards> Rewards { get; set; }
}
