using Microsoft.Extensions.Logging.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using System.Diagnostics;

namespace MQTTnet.WorkerSample;




internal static class MqttClientFactoryProvider
{
    const int maxRetries = 20;
    private static int currentRetries = maxRetries;

    public static Func<IServiceProvider, IMqttClient> _mqttClientFactory = service =>
    {
        IConfiguration? config = service.GetService<IConfiguration>();
        bool mqttDiag = config!.GetValue<bool>("mqttDiag");
        IMqttClient mqttClient;
        ILogger logger = LoggerFactory.Create(builder => builder.AddConfiguration(config!)).CreateLogger<Worker>();

        if (mqttDiag)
        {
            Trace.Listeners.Add(new ConsoleTraceListener()) ;
            mqttClient = new MqttClientFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger());
        }
        else
        {
            mqttClient = new MqttClientFactory().CreateMqttClient();
        }

        mqttClient.DisconnectedAsync += async e =>
        {
            logger.LogWarning("Mqtt Client Disconnected with reason {r}", e.Reason);
            while (currentRetries > 0 && !mqttClient.IsConnected)
            {
                int delay = (maxRetries - currentRetries--) * 1000;
                await Task.Delay(delay);
                logger.LogWarning("Waiting to reconnect " + delay);
                await mqttClient.ReconnectAsync();
                if (mqttClient.IsConnected)
                {
                    logger.LogInformation("Client Reconnected");
                    currentRetries = maxRetries;
                }
            }
        };
        return mqttClient;
    };

}

public class MqttNetTraceLogger
{
    [DebuggerStepThrough()]
    public static MqttNetEventLogger CreateTraceLogger()
    {
        MqttNetEventLogger logger = new();
        logger.LogMessagePublished += (s, e) =>
        {
            string trace = $">> [{e.LogMessage.Timestamp:O}] [{e.LogMessage.ThreadId}]: {e.LogMessage.Message}";
            if (e.LogMessage.Exception != null)
            {
                trace += Environment.NewLine + e.LogMessage.Exception.ToString();
            }
            Trace.TraceInformation(trace);
        };
        return logger;
    }
}