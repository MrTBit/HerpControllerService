using HerpControllerService.Entities;
using HerpControllerService.Enums;
using HerpControllerService.Models;
using HerpControllerService.Models.Device;
using HerpControllerService.Services;
using HerpControllerService.SignalR;
using Newtonsoft.Json;

namespace HerpControllerService.mqtt.Processors;

public class ReceivedSensorDataProcessor(ILogger<ReceivedSensorDataProcessor> logger, DeviceService deviceService, SensorService sensorService, SensorReadingService sensorReadingService, RealTimeHubSender hubSender, AlertService alertService)
{
    public async Task Process(MqttService mqttService, string deviceHardwareName, string message)
    {
        var sensorData = JsonConvert.DeserializeObject<DeviceSensorDataModel>(message);

        if (sensorData == null)
        {
            logger.LogWarning("Received malformed sensor data from device {deviceId}.", deviceHardwareName);
            return;
        }
        
        var device = await deviceService.GetDeviceByHardwareId(deviceHardwareName);

        if (device == null)
        {
            logger.LogWarning("Received message from device {deviceId} not found in db.", deviceHardwareName);
            return;
        }
        
        mqttService.AddOrResetTimer(device);

        var realTimeSensorDataModel = new RealTimeSensorDataModel
        {
            DeviceId = device.Id,
            SensorData = []
        };
        
        foreach (var ds18BSensorData in sensorData.Ds18BSensors ?? [])
        {
            var sensor = device.Sensors?.FirstOrDefault(s => s.HardwareId == ds18BSensorData.DeviceAddress);
            
            sensor ??= await sensorService.CreateSensor(
                device, 
                ds18BSensorData.DeviceAddress, 
                ds18BSensorData.DeviceAddress,
                SensorType.DS18B);

            await CheckSensorAlert(sensor, ds18BSensorData.Temperature, null);
            
            await sensorReadingService.CreateSensorReading(
                sensor, 
                ds18BSensorData.Temperature,
                SensorReadingType.TEMPERATURE,
                false);
            
            realTimeSensorDataModel.SensorData.Add(new RealTimeSensorReadingModel
            {
                SensorId = sensor.Id,
                Temperature = ds18BSensorData.Temperature
            });
        }

        foreach (var dhtSensorData in sensorData.DhtSensors ?? [])
        {
            var sensor = device.Sensors?.FirstOrDefault(s => s.HardwareId == dhtSensorData.Pin.ToString());
            
            sensor ??= await sensorService.CreateSensor(
                device,
                dhtSensorData.Pin.ToString(),
                dhtSensorData.Pin.ToString(),
                SensorType.DHT
            );

            await CheckSensorAlert(sensor, dhtSensorData.Temperature, dhtSensorData.Humidity);
            
            await sensorReadingService.CreateSensorReading(
                sensor,
                dhtSensorData.Temperature,
                SensorReadingType.TEMPERATURE,
                false);

            await sensorReadingService.CreateSensorReading(
                sensor,
                dhtSensorData.Humidity,
                SensorReadingType.HUMIDITY,
                false);
            
            realTimeSensorDataModel.SensorData.Add(new RealTimeSensorReadingModel
            {
                SensorId = sensor.Id,
                Humidity = dhtSensorData.Humidity,
                Temperature = dhtSensorData.Temperature
            });
        }

        await hubSender.BroadcastRealTimeSensorData(realTimeSensorDataModel);
        
        await sensorReadingService.SaveChanges();
    }

    private async Task CheckSensorAlert(SensorEntity sensor, double temperature, double? humidity)
    {
        var sensorAlertData = new List<SensorAlertData>();
        
        if (sensor.MinimumTemperature.HasValue && temperature < sensor.MinimumTemperature)
        {
            sensorAlertData.Add(new SensorAlertData{AlertType = AlertType.LOW_TEMP, Value = temperature});
        } else if (sensor.MaximumTemperature.HasValue && temperature > sensor.MaximumTemperature)
        {
            sensorAlertData.Add(new SensorAlertData{AlertType = AlertType.HIGH_TEMP, Value = temperature});
        } else if (humidity.HasValue)
        {
            if (sensor.MinimumHumidity.HasValue && humidity < sensor.MinimumHumidity)
            {
                sensorAlertData.Add(new SensorAlertData{AlertType = AlertType.LOW_HUMIDITY, Value = humidity.Value});
            } else if (sensor.MaximumHumidity.HasValue && humidity > sensor.MaximumHumidity)
            {
                sensorAlertData.Add(new SensorAlertData{AlertType = AlertType.HIGH_HUMIDITY, Value = humidity.Value});
            }
        }

        if (sensorAlertData.Count == 0)
        {
            return;
        }

        foreach (var alertData in sensorAlertData)
        {
            await alertService.CreateSensorAlert(sensor, alertData.AlertType, alertData.Value);
        }
    }
    
    private record SensorAlertData
    {
        public AlertType AlertType { get; init; }
        public double Value { get; init; }
    }
}