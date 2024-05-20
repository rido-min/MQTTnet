using Microsoft.VisualBasic;
using MQTTnet;
using MQTTnet.Client;

namespace MQTTnet.WorkerSample;

internal class ConsumerService(IMqttClient mqttClient, ILogger<ConsumerService> logger)
{
    internal Action<string>? OnMessageReceived { get; set; }

    internal async Task StartAsync(string topic, CancellationToken cancellationToken = default)
    {
        mqttClient.ApplicationMessageReceivedAsync += async msg =>
        {
            msg.AutoAcknowledge = false;

            try
            {
                    OnMessageReceived?.Invoke(msg.ApplicationMessage.ConvertPayloadToString());
                    logger.LogInformation("Acking message {0}", msg.PacketIdentifier);
                    await msg.AcknowledgeAsync(cancellationToken);
               
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process message " + msg.PacketIdentifier);
                //await msg.AcknowledgeAsync(cancellationToken);
            }
           

        };
        await mqttClient.SubscribeAsync(topic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, cancellationToken);
        
    }
}
