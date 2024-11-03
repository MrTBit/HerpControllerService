using HerpControllerService.Database;
using HerpControllerService.Entities;

namespace HerpControllerService.Services;

public class TimerPinStateService(HerpControllerDbContext db)
{
    public async Task CreateTimerPinState(DeviceEntity device, int pin, bool state)
    {
        db.TimerPinStates.Add(new TimerPinStateEntity
        {
            DeviceId = device.Id,
            Pin = pin,
            State = state,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }
}