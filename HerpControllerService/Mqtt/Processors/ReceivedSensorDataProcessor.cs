using HerpControllerService.Enums;
using HerpControllerService.Models;
using HerpControllerService.Models.Device;
using HerpControllerService.Services;
using HerpControllerService.SignalR;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace HerpControllerService.mqtt.Processors;

public class ReceivedSensorDataProcessor(ILogger<ReceivedSensorDataProcessor> logger, DeviceService deviceService, SensorService sensorService, SensorReadingService sensorReadingService, RealTimeHubSender hubSender)
{
    public async Task Process(string deviceHardwareName, string message)
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

        var realTimeSensorDataModel = new RealTimeSensorDataModel
        {
            deviceId = device.Id,
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
}