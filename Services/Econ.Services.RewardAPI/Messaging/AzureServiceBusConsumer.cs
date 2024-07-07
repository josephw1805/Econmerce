using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace Econ.Services.RewardAPI;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
  private readonly string serviceBusConnectionString;
  private readonly string orderCreatedTopic;
  private readonly string orderCreatedRewardSubscription;
  private readonly IConfiguration _configuration;
  private readonly ServiceBusProcessor _rewardProcessor;
  private readonly RewardService _rewardService;

  public AzureServiceBusConsumer(IConfiguration configuration, RewardService rewardService)
  {
    _configuration = configuration;
    _rewardService = rewardService;
    serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString")!;
    orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic")!;
    orderCreatedRewardSubscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Rewards_Subscription")!;
    var client = new ServiceBusClient(serviceBusConnectionString);
    _rewardProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedRewardSubscription);
  }

  public async Task Start()
  {
    _rewardProcessor.ProcessMessageAsync += OnNewOrderRewardsRequestReceived;
    _rewardProcessor.ProcessErrorAsync += ErrorHandler;
    await _rewardProcessor.StartProcessingAsync();
  }

  public async Task Stop()
  {
    await _rewardProcessor.StopProcessingAsync();
    await _rewardProcessor.DisposeAsync();
  }

  private async Task OnNewOrderRewardsRequestReceived(ProcessMessageEventArgs args)
  {
    // this is where you will receive message
    var message = args.Message;
    var body = Encoding.UTF8.GetString(message.Body);

    RewardsMessage objMessage = JsonConvert.DeserializeObject<RewardsMessage>(body)!;
    try
    {
      await _rewardService.UpdateRewards(objMessage);
      await args.CompleteMessageAsync(args.Message);
    }
    catch (Exception)
    {
      throw;
    }
  }

  private Task ErrorHandler(ProcessErrorEventArgs args)
  {
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
  }
}
