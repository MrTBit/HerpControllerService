using System.Net;
using HerpControllerService.Database;
using HerpControllerService.Entities;
using HerpControllerService.Enums;
using HerpControllerService.Exceptions;
using HerpControllerService.Models.API;
using Microsoft.EntityFrameworkCore;

namespace HerpControllerService.Services;

public class SensorService(HerpControllerDbContext db)
{
    public async Task<SensorEntity> CreateSensor(DeviceEntity device, string hardwareId, string name, SensorType type)
    {
        var sensor = new SensorEntity
        {
            DeviceId = device.Id,
            Name = name,
            HardwareId = hardwareId,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };

        db.Sensors.Add(sensor);
        await db.SaveChangesAsync();

        return sensor;
    }

    public async Task<SensorEntity> UpdateSensor(SensorModel model)
    {
        var sensor = await db.Sensors.FirstOrDefaultAsync(s => s.Id == model.Id);

        if (sensor == null)
        {
            throw new HerpControllerException(HttpStatusCode.BadRequest, "Sensor not found.");
        }
        
        sensor.Name = model.Name;
        sensor.MinimumTemperature = model.MinimumTemperature;
        sensor.MaximumTemperature = model.MaximumTemperature;
        sensor.MinimumHumidity = model.MinimumHumidity;
        sensor.MaximumHumidity = model.MaximumHumidity;
        sensor.ModifiedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return sensor;
    }
}