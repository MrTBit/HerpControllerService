using System.Net;
using HerpControllerService.Database;
using HerpControllerService.Entities;
using HerpControllerService.Enums;
using HerpControllerService.Exceptions;
using HerpControllerService.Models.API;
using Microsoft.EntityFrameworkCore;

namespace HerpControllerService.Services;

public class AlertService(HerpControllerDbContext db, ILogger<AlertService> logger)
{
    public async Task<List<AlertEntity>> GetActiveAlerts() =>
        await db.Alerts
            .Include(a => a.Device)
            .Include(a => a.Sensor)
            .Where(a => a.Status == AlertStatus.ACTIVE)
            .ToListAsync();
    
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

        if (alert.Status != AlertStatus.ACTIVE)
        {
            logger.LogWarning("Attempted to acknowledge an alert that is not active: {alertId}", alertId);
            return;
        }

        alert.Status = AlertStatus.ACKNOWLEDGED;
        alert.ModifiedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }
}