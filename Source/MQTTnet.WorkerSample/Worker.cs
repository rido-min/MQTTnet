
using MQTTnet.Client;

namespace MQTTnet.WorkerSample;

public class Worker(IMqttClient mqttClient, IServiceProvider provider, ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectAsync(stoppingToken);

        ConsumerService consumer = provider.GetService<ConsumerService>()!;
        consumer.OnMessageReceived += async m =>
        {
            if (m == "fail")
            {
              await Task.Delay(5000, stoppingToken);
              throw new Exception("Failed to process message");
            }
            logger.LogInformation("Received message {m}", m);
        };
        await consumer.StartAsync("test/mqtt", stoppingToken);

        ProducerService producer = provider.GetService<ProducerService>()!;


        int counter = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            if (counter % 5 == 0)
            {
                await producer.SendMessageAsync("test/mqtt", "fail");
            }
            else
            {
                await producer.SendMessageAsync("test/mqtt", "hello mqtt " + counter++);
            }
            await producer.SendMessageAsync("test/mqtt", "hello mqtt " + counter++);
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ConnectAsync(CancellationToken stoppingToken)
    {
        string cs = configuration.GetConnectionString("Mq")!;
        // MqttConnectionSettings mcs = MqttConnectionSettings.FromConnectionString(cs);
        MqttClientConnectResult connAck = await mqttClient.ConnectAsync(new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883)
            .WithClientId("Worker")
            .WithCleanStart(false)
            .WithSessionExpiryInterval(3600)
            .Build(), stoppingToken);

        if (connAck.ResultCode != MqttClientConnectResultCode.Success)
        {
            logger.LogError("Failed to connect to MQTT broker: {connAck.ResultCode}", connAck.ResultCode);
            return;
        }
        else
        {
            logger.LogInformation("Connected to {0} ClientId {1} persistent session {2} {3}",
                "localhost:1883", mqttClient.Options.ClientId, connAck.IsSessionPresent, connAck.SessionExpiryInterval);
        }
    }
}
