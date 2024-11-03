using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HerpControllerService.Models.API;

namespace HerpControllerService.Entities;

[Table("devices")]
public class DeviceEntity : BaseEntity
{
    [Column("hardware_id")]
    [Required]
    public string HardwareId { get; set; } = null!;
    
    [Column("name")]
    [Required]
    public string Name { get; set; } = null!; // User set name
    
    public ICollection<SensorEntity>? Sensors { get; set; }

    public static implicit operator DeviceModel(DeviceEntity entity)
    {
        return new DeviceModel
        {
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            ModifiedAt = entity.ModifiedAt,
            HardwareId = entity.HardwareId,
            Name = entity.Name,
            Sensors = entity.Sensors?.Select(s => (SensorModel)s).ToList() ?? []
        };
    }
}