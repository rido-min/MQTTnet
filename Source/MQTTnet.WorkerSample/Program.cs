using MQTTnet.WorkerSample;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddSingleton(MqttClientFactoryProvider._mqttClientFactory)
    .AddTransient<ProducerService>()
    .AddTransient<ConsumerService>()
    .AddHostedService<Worker>();

IHost host = builder.Build();
host.Run();
