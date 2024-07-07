namespace Econ.Services.EmailAPI;

public interface IAzureServiceBusConsumer
{
  Task Start();
  Task Stop();
}
