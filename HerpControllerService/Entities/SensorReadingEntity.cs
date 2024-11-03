using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HerpControllerService.Enums;

namespace HerpControllerService.Entities;

[Table("sensor_readings")]
public class SensorReadingEntity : BaseEntity
{
    [Column("sensor_id")]
    [Required]
    public long SensorId { get; set; }
    public SensorEntity? Sensor { get; set; }
    
    [Column("type")]
    [Required]
    public SensorReadingType Type { get; set; }
    
    [Column("value")]
    [Required]
    public double Value { get; set; }
}