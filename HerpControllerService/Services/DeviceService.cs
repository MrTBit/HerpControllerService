using System.Net;
using HerpControllerService.Database;
using HerpControllerService.Entities;
using HerpControllerService.Exceptions;
using HerpControllerService.Models.API;
using Microsoft.EntityFrameworkCore;

namespace HerpControllerService.Services;

public class DeviceService(HerpControllerDbContext db)
{
    public async Task<DeviceEntity?> GetDeviceByHardwareId(string hardwareId) =>
        await db.Devices
            .Include(d => d.Sensors)
            .FirstOrDefaultAsync(d => d.HardwareId == hardwareId);

    public async Task<DeviceEntity> CreateDevice(string hardwareId)
    {
        var device = new DeviceEntity
        {
            HardwareId = hardwareId,
            Name = hardwareId,
            CreatedAt = DateTime.UtcNow
        };
        
        db.Devices.Add(device);
        await db.SaveChangesAsync();

        return device;
    }

    /// <summary>
    /// Gets devices by id(s), if none are provided, returns all devices.
    /// </summary>
    /// <returns>A list of <see cref="DeviceEntity"/></returns>
    public async Task<List<DeviceEntity>> GetDevices(List<long>? deviceIds)
    {
        if ((deviceIds?.Count ?? 0) > 0)
        {
            return await db.Devices
                .Include(d => d.Sensors)
                .Where(d => deviceIds!.Contains(d.Id)).ToListAsync();
        }

        return await db.Devices.Include(d => d.Sensors).ToListAsync();
    }

    public async Task<DeviceEntity> UpdateDevice(DeviceModel deviceModel)
    {
        var existing = await db.Devices.FirstOrDefaultAsync(d => d.Id == deviceModel.Id);

        if (existing == null)
        {
            throw new HerpControllerException(HttpStatusCode.BadRequest, "Device not found");
        }
        
        existing.Name = deviceModel.Name;
        existing.ModifiedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return existing;
    }
}