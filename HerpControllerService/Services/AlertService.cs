using System.Net;
using HerpControllerService.Database;
using HerpControllerService.Entities;
using HerpControllerService.Enums;
using HerpControllerService.Exceptions;
using HerpControllerService.Models.API;
using Microsoft.EntityFrameworkCore;

namespace HerpControllerService.Services;

public class AlertService(HerpControllerDbContext db, ILogger<AlertService> logger, TelegramService telegramService)
{
    public async Task<List<AlertEntity>> GetActiveAlerts(AlertType? type = null)
    {
        var query = db.Alerts
            .Include(a => a.Device)
            .Include(a => a.Sensor)
            .Where(a => a.Status != AlertStatus.DISMISSED);

        if (type != null)
        {
            query = query.Where(a => a.Type == type);
        }
        
        return await query.ToListAsync();
    }
    
    public async Task<AlertEntity?> GetAlertById(long alertId) => await db.Alerts
        .Include(a => a.Device)
        .Include(a => a.Sensor)
        .FirstOrDefaultAsync(a => a.Id == alertId);
    
    public async Task<AlertEntity> Update(AlertModel model)
    {
        var alert = await GetAlertById(model.Id);

        if (alert == null)
        {
            throw new HerpControllerException(HttpStatusCode.BadRequest, "Alert not found");
        }
        
        alert.Status = model.Status;
        alert.ModifiedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return alert;
    }
    
    public async Task AcknowledgeAlert(long alertId)
    {
        var alert = await GetAlertById(alertId);

        if (alert == null)
        {
            logger.LogWarning("Attempted to acknowledge unknown alert: {alertId}", alertId);
            return;
        }

        if (alert.Status != AlertStatus.CREATED)
        {
            logger.LogWarning("Attempted to acknowledge an alert that is not active: {alertId}", alertId);
            return;
        }

        alert.Status = AlertStatus.ACKNOWLEDGED;
        alert.ModifiedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    public async Task CreateDeviceNotReportingAlert(DeviceEntity device)
    {
        var existing = await db.Alerts.FirstOrDefaultAsync(a => a.DeviceId == device.Id && a.Type == AlertType.DEVICE_MISSING && a.Status != AlertStatus.DISMISSED);

        if (existing != null)
        {
            switch (existing.Status)
            {
                case AlertStatus.ACKNOWLEDGED:
                    return;
                case AlertStatus.CREATED:
                    await telegramService.SendAlertAsync(existing.Id, existing.Message, true);
                    return;
            }
        }
        
        var alert = new AlertEntity
        {
            CreatedAt = DateTime.UtcNow,
            DeviceId = device.Id,
            Status = AlertStatus.CREATED,
            Type = AlertType.DEVICE_MISSING,
            Message = $"Device {device.Name} is not sending data."
        };
        
        db.Alerts.Add(alert);
        await db.SaveChangesAsync();

        await telegramService.SendAlertAsync(alert.Id, alert.Message, false);
    }

    public async Task CreateSensorAlert(SensorEntity sensor, AlertType alertType, double currentValue)
    {
        var existing = await db.Alerts.FirstOrDefaultAsync(a => a.SensorId == sensor.Id && a.Type == alertType && a.Status != AlertStatus.DISMISSED);

        string message;
        
        switch (alertType)
        {
            case AlertType.LOW_TEMP:
            {
                message = $"Sensor {sensor.Name} reporting low temperature: {currentValue}";
                break;
            }
            case AlertType.HIGH_TEMP:
            {
                message = $"Sensor {sensor.Name} reporting high temperature: {currentValue}";
                break;   
            }
            case AlertType.LOW_HUMIDITY:
            {
                message = $"Sensor {sensor.Name} reporting low humidity: {currentValue}";
                break;
            }
            case AlertType.HIGH_HUMIDITY:
            {
                message = $"Sensor {sensor.Name} reporting high humidity: {currentValue}";
                break;
            }
            case AlertType.DEVICE_MISSING:
            default:
            {
                return;
            }
        }
        
        if (existing != null)
        {
            switch (existing.Status)
            {
                case AlertStatus.ACKNOWLEDGED:
                    return;
                case AlertStatus.CREATED:
                    existing.Message = message;
                    existing.ModifiedAt = DateTime.UtcNow;

                    await db.SaveChangesAsync();
                    
                    await telegramService.SendAlertAsync(existing.Id, message, true);
                    return;
            }
        }
        
        var alert = new AlertEntity
        {
            CreatedAt = DateTime.UtcNow,
            SensorId = sensor.Id,
            Status = AlertStatus.CREATED,
            Type = alertType,
            Message = message
        };
        
        db.Alerts.Add(alert);
        await db.SaveChangesAsync();

        await telegramService.SendAlertAsync(alert.Id, alert.Message, false);
    }
}