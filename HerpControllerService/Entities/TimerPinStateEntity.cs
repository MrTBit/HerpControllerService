using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HerpControllerService.Entities;

[Table("timer_pin_states")]
public class TimerPinStateEntity : BaseEntity
{
    [Column("device_id")]
    [Required]
    public long DeviceId { get; set; }
    public DeviceEntity? Device { get; set; }
    
    [Column("state")]
    [Required]
    public bool State { get; set; }
    
    [Column("pin")]
    [Required]
    public int Pin { get; set; }
}