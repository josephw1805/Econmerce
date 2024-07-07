
namespace Econ.Services.RewardAPI;

public static class ApplicationBuilderExtension
{
  private static IAzureServiceBusConsumer? ServiceBusConsumer { get; set; }
  public static IApplicationBuilder UseAzureServiceBusConsumer(this IApplicationBuilder app)
  {
    ServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>();
    var hostApplicationLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();
    if (hostApplicationLife != null)
    {
      hostApplicationLife.ApplicationStarted.Register(OnStart);
      hostApplicationLife.ApplicationStopped.Register(OnStop);
    }
    return app;
  }

  private static void OnStop()
  {
    ServiceBusConsumer?.Stop();
  }

  private static void OnStart()
  {
    ServiceBusConsumer?.Start();
  }
}
