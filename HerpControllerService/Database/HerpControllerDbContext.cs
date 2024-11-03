using HerpControllerService.Entities;
using Microsoft.EntityFrameworkCore;

namespace HerpControllerService.Database;

public class HerpControllerDbContext(DbContextOptions<HerpControllerDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<DeviceEntity> Devices { get; set; }
    public DbSet<SensorEntity> Sensors { get; set; }
    public DbSet<SensorReadingEntity> SensorReadings { get; set; }
    public DbSet<TimerPinStateEntity> TimerPinStates { get; set; }
    
}