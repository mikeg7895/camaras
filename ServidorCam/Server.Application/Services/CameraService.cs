using Microsoft.EntityFrameworkCore;
using Server.Application.Interfaces;
using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;

namespace Server.Application.Services;

public class CameraService(ICameraRepository repository) : GenericService<Camera>(repository), ICameraService
{
    private readonly ICameraRepository _cameraRepository = repository;

    public async Task<IEnumerable<Camera>> GetByUserIdAsync(int userId) 
        => await _cameraRepository.GetAll().Where(c => c.UserId == userId).ToListAsync();

    public async Task<Camera?> GetByDeviceIdAsync(Guid deviceId) 
        => await _cameraRepository.GetAll().Where(c => c.DeviceId == deviceId).FirstOrDefaultAsync(); 

}