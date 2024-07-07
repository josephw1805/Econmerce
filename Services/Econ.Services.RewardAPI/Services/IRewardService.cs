namespace Econ.Services.RewardAPI;

public interface IRewardService
{
  Task UpdateRewards(RewardsMessage rewardsMessage);
}
