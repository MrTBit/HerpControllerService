using System.Text;
using HerpControllerService.Database;
using HerpControllerService.Enums;
using HerpControllerService.Models.Device;
using HerpControllerService.mqtt.Processors;
using HerpControllerService.Services;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using Newtonsoft.Json;

namespace HerpControllerService.mqtt;

public class MqttService : BackgroundService
{
    private readonly MqttClientFactory _mqttFactory;
    private readonly IMqttClient _client;
    
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;

    private const string ALL_DEVICE_COMMAND_TOPIC = "herpcontroller/devices/all_commands";
    
    public MqttService(ILogger<MqttService> logger, IConfiguration configuration, IServiceProvider services)
    {
        _mqttFactory = new MqttClientFactory();
        _client = _mqttFactory.CreateMqttClient();

        _logger = logger;
        _configuration = configuration;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_configuration["MQTT:Server"])
            .WithCredentials(_configuration["MQTT:Username"], _configuration["MQTT:Password"])
            .WithTlsOptions(options => options.UseTls(false))
            .Build();
        
        _logger.LogInformation("Connecting to MQTT broker...");

        var success = false;

        while (!success)
        {
            try
            {
                await _client.ConnectAsync(mqttClientOptions, CancellationToken.None);
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to connect to MQTT broker, retrying...");
            }
        }

        List<string> topics = ["herpcontroller/devices/all_data"];

        using (var scope = _services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HerpControllerDbContext>();
            
            var deviceNames = await db.Devices.Select(d => d.HardwareId).ToListAsync(stoppingToken);

            foreach (var deviceName in deviceNames)
            {
                topics.Add($"herpcontroller/devices/{deviceName}/sensor_data");
                topics.Add($"herpcontroller/devices/{deviceName}/timer_data");
                topics.Add($"herpcontroller/devices/{deviceName}/data");
            }
        }
        
        // Fetch current topics and subscribe.
        await SubscribeToTopics(topics);
        
        // Start
        StartProcessing(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var mqttClientDisconnectOptions = _mqttFactory.CreateClientDisconnectOptionsBuilder().Build();

        await _client.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
        
        _client.Dispose();
        
        await base.StopAsync(cancellationToken);
    }

    public async Task SubscribeToTopics(List<string> topics)
    {
        _logger.LogInformation("Subscribing to topics...");
        
        var subscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder();

        foreach (var topic in topics)
        {
            subscribeOptions.WithTopicFilter(topic);
        }
        
        await _client.SubscribeAsync(subscribeOptions.Build(), CancellationToken.None);
        
        _logger.LogInformation("Successfully subscribed to topics.");
    }

    public async Task SubscribeToNewDeviceTopics(string deviceName)
    {
        var topics = new List<string>();
        topics.Add($"herpcontroller/devices/{deviceName}/sensor_data");
        topics.Add($"herpcontroller/devices/{deviceName}/timer_data");
        topics.Add($"herpcontroller/devices/{deviceName}/data");
        await SubscribeToTopics(topics);
    }

    private async Task PublishMessage(string topic, string message)
    {
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(message)
            .Build();

        await _client.PublishAsync(mqttMessage, CancellationToken.None);
    }

    private string GetDeviceCommandTopic(string deviceHardwareName) => $"herpcontroller/devices/{deviceHardwareName}/commands";
    
    public async Task SendRequestSensorDataCommand(string deviceHardwareName)
    {
        await PublishMessage(GetDeviceCommandTopic(deviceHardwareName),
            JsonConvert.SerializeObject(new { command = DeviceCommand.SEND_SENSOR_DATA }));
    }

    public async Task SendNewDeviceConfig(string deviceHardwareName, DeviceConfigModel config)
    {
        await PublishMessage(GetDeviceCommandTopic(deviceHardwareName),
            JsonConvert.SerializeObject(new { command = DeviceCommand.UPDATE_CONFIG, config }));
    }
    
    public async Task SendDeviceChangeSensorPollingIntervalCommand(string deviceHardwareName, long interval)
    {
        await PublishMessage(GetDeviceCommandTopic(deviceHardwareName),
            JsonConvert.SerializeObject(new { command = DeviceCommand.SEND_SENSOR_INTERVAL, interval }));
    }

    public async Task SendAnnouncePresenceCommand(string deviceHardwareName, string requesterId)
    {
        await PublishMessage(GetDeviceCommandTopic(deviceHardwareName),
            JsonConvert.SerializeObject(new { command = DeviceCommand.ANNOUNCE_PRESENCE, requesterId }));
    }

    public async Task SendAnnouncePresenceCommandToAll(string? requesterId)
    {
        await PublishMessage(ALL_DEVICE_COMMAND_TOPIC,
            JsonConvert.SerializeObject(new { command = DeviceCommand.ANNOUNCE_PRESENCE, requesterId }));
    }

    public async Task SendSendCurrentConfigCommand(string deviceHardwareName, string requesterId)
    {
        await PublishMessage(GetDeviceCommandTopic(deviceHardwareName),
            JsonConvert.SerializeObject(new
                { command = DeviceCommand.SEND_CURRENT_CONFIG, requesterId }));
    }

    public async Task SendSendCurrentTimerPinStatesCommand(string deviceHardwareName, string requesterId)
    {
        await PublishMessage(GetDeviceCommandTopic(deviceHardwareName),
            JsonConvert.SerializeObject(new { command = DeviceCommand.SEND_TIMER_STATES, requesterId }));
    }

    private void StartProcessing(CancellationToken cancellationToken)
    {
        var concurrent = new SemaphoreSlim(2, 4);

        _client.ApplicationMessageReceivedAsync += async ea =>
        {
            await concurrent.WaitAsync(cancellationToken).ConfigureAwait(false);

            await Task.Run(ProcessAsync, cancellationToken);
            return;

            async Task ProcessAsync()
            {
                try
                {
                    using var scope = _services.CreateScope();

                    var message = Encoding.UTF8.GetString(ea.ApplicationMessage.Payload);
                    _logger.LogInformation("Received message: {message}", message);

                    var topic = ea.ApplicationMessage.Topic;
                    if (topic == "herpcontroller/devices/all_data")
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<ReceivedPresenceDataProcessor>();
                        await processor.Process(this, message);
                    }
                    else if (topic.StartsWith("herpcontroller/devices/"))
                    {
                        var parts = topic.Split('/');
                        var deviceHardwareName = parts[2];
                        var deviceTopic = parts[3];

                        switch (deviceTopic)
                        {
                            case "sensor_data":
                            {
                                var processor = scope.ServiceProvider.GetRequiredService<ReceivedSensorDataProcessor>();
                                await processor.Process(deviceHardwareName, message);
                                break;
                            }
                            case "timer_data":
                            {
                                var processor = scope.ServiceProvider.GetRequiredService<ReceivedTimerDataProcessor>();
                                await processor.Process(deviceHardwareName, message);
                                break;
                            }
                            case "data": // Direct responses
                            {
                                var processor = scope.ServiceProvider
                                    .GetRequiredService<ReceivedDirectResponseDataProcessor>();
                                await processor.Process(deviceHardwareName, message);
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An unexpected error occurred while processing received device message.");
                }
                finally
                {
                    await ea.AcknowledgeAsync(cancellationToken);
                    concurrent.Release();
                }
            }
        };
    }
}