namespace Econ.Services.RewardAPI;

public interface IAzureServiceBusConsumer
{
  Task Start();
  Task Stop();
}
