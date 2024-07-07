using Microsoft.EntityFrameworkCore;

namespace Econ.Services.EmailAPI;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<EmailLogger> EmailLoggers { get; set; }
}
