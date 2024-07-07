namespace Econ.Services.EmailAPI;

public interface IEmailService
{
  Task EmailCartAndLog(CartDto cartDto);
  Task RegisterUserEmailAndLog(string email);
  Task LogOrderPlaced(RewardsMessage rewardsDto);
}
