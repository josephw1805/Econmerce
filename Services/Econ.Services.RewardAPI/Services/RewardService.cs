using Microsoft.EntityFrameworkCore;

namespace Econ.Services.RewardAPI;

public class RewardService(DbContextOptions<AppDbContext> dbOptions) : IRewardService
{
  private readonly DbContextOptions<AppDbContext> _dbOptions = dbOptions;

  public async Task UpdateRewards(RewardsMessage rewardsMessage)
  {
    try
    {
      Rewards rewards = new()
      {
        OrderId = rewardsMessage.OrderId,
        RewardsActivity = rewardsMessage.RewardsActivity,
        UserId = rewardsMessage.UserId,
        RewardsDate = DateTime.Now
      };
      await using var _db = new AppDbContext(_dbOptions);
      await _db.Rewards.AddAsync(rewards);
      await _db.SaveChangesAsync();
    }
    catch (Exception)
    {
      throw;
    }
  }
}
