using System.ComponentModel.DataAnnotations.Schema;
using HerpControllerService.Enums;
using HerpControllerService.Models.API;

namespace HerpControllerService.Entities;

[Table("alerts")]
public class AlertEntity : BaseEntity
{
    [Column("device_id")]
    public long? DeviceId { get; set; }
    public DeviceEntity? Device { get; set; }
    
    [Column("sensor_id")]
    public long? SensorId { get; set; }
    public SensorEntity? Sensor { get; set; }
    
    [Column("type", TypeName = "varchar(64)")]
    public AlertType Type { get; set; }
    
    [Column("message")]
    public string Message { get; set; }
    
    [Column("status", TypeName = "varchar(64)")]
    public AlertStatus Status { get; set; }

    public static implicit operator AlertModel(AlertEntity entity) => new()
    {
        Id = entity.Id,
        CreatedAt = entity.CreatedAt,
        ModifiedAt = entity.ModifiedAt,
        DeviceId = entity.DeviceId,
        Device = entity.Device,
        SensorId = entity.SensorId,
        Sensor = entity.Sensor,
        Type = entity.Type,
        Message = entity.Message,
        Status = entity.Status
    };
}