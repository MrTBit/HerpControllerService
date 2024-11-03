using HerpControllerService.Database;
using HerpControllerService.Entities;
using HerpControllerService.Enums;

namespace HerpControllerService.Services;

public class SensorReadingService(HerpControllerDbContext db)
{
    public async Task CreateSensorReading(SensorEntity sensor, double value, SensorReadingType type, bool saveChanges = true)
    {
        db.SensorReadings.Add(new SensorReadingEntity
        {
            SensorId = sensor.Id,
            Type = type,
            Value = value,
            CreatedAt = DateTime.UtcNow
        });

        if (!saveChanges)
        {
            return;
        }

        await db.SaveChangesAsync();
    }
    
    public async Task SaveChanges() => await db.SaveChangesAsync();
}