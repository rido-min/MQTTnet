using MQTTnet.Client;

namespace MQTTnet.WorkerSample;

internal class ProducerService(IMqttClient mqttClient, ILogger<ProducerService> logger)
{
    public Task SendMessageAsync(string topic, string message, CancellationToken cancellationToken = default)
    {
        if (mqttClient.IsConnected)
        {
            return mqttClient.PublishStringAsync(topic, message, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false, cancellationToken);
        }
        else
        {
            logger.LogWarning("Missing one message");
            return Task.CompletedTask;
        }
    }
}
