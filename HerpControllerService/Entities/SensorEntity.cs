using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HerpControllerService.Enums;
using HerpControllerService.Models.API;

namespace HerpControllerService.Entities;

[Table("sensors")]
public class SensorEntity : BaseEntity
{
    [Column("device_id")]
    [Required]
    public long DeviceId { get; set; }
    public DeviceEntity? Device { get; set; }
    
    [Column("hardware_id")]
    [Required]
    public string HardwareId { get; set; } = null!;
    
    [Column("name")]
    [Required]
    public string Name { get; set; } = null!;
    
    [Column("type", TypeName = "varchar(64)")]
    [Required]
    public SensorType Type { get; set; }
    
    [Column("min_t")]
    public double? MinimumTemperature { get; set; }
    
    [Column("max_t")]
    public double? MaximumTemperature { get; set; }
    
    [Column("min_h")]
    public double? MinimumHumidity { get; set; }
    
    [Column("max_h")]
    public double? MaximumHumidity { get; set; }

    public static implicit operator SensorModel(SensorEntity entity)
    {
        return new SensorModel
        {
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            ModifiedAt = entity.ModifiedAt,
            HardwareId = entity.HardwareId,
            Name = entity.Name,
            Type = entity.Type
        };
    }
}