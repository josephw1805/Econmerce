
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Econ.Services.EmailAPI;

public class EmailService(DbContextOptions<AppDbContext> dbOptions) : IEmailService
{
  private readonly DbContextOptions<AppDbContext> _dbOptions = dbOptions;

  public async Task EmailCartAndLog(CartDto cartDto)
  {
    StringBuilder message = new();
    message.AppendLine("<br>Cart Email Requested");
    message.AppendLine("<br>Total " + cartDto.CartHeader.CartTotal);
    message.Append("<br>");
    message.Append("<ul>");
    foreach (var item in cartDto.CartDetails!)
    {
      message.Append("<li>");
      message.Append(item.Product!.Name + " x " + item.Count);
      message.Append("</li>");
    }
    message.Append("</ul>");

    await LogAndEmail(message.ToString(), cartDto.CartHeader.Email!);
  }

  public async Task LogOrderPlaced(RewardsMessage rewardsDto)
  {
    string message = "New Order Placed. <br> Order ID : " + rewardsDto.OrderId;
    await LogAndEmail(message, "wdia@gmail.com");
  }

  public async Task RegisterUserEmailAndLog(string email)
  {
    string message = "User Registration Successfull. <br> Email: " + email;
    await LogAndEmail(message, "wdia@gmail.com");
  }

  private async Task<bool> LogAndEmail(string message, string email)
  {
    try
    {
      EmailLogger emailLog = new()
      {
        Email = email,
        EmailSent = DateTime.Now,
        Message = message
      };
      await using var _db = new AppDbContext(_dbOptions);
      await _db.EmailLoggers.AddAsync(emailLog);
      await _db.SaveChangesAsync();
      return true;
    }
    catch (Exception)
    {
      return false;
    }
  }
}
