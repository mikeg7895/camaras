using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Application.Interfaces;
using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;

namespace Server.Application.Services;

public class CameraService(ICameraRepository repository) : GenericService<Camera>(repository), ICameraService
{
    private readonly ICameraRepository _cameraRepository = repository;

    public async Task<IEnumerable<Camera>> GetByUserIdAsync(int userId) 
        => await _cameraRepository.GetByUserIdAsync(userId);

    public async Task<Camera?> GetByDeviceIdAsync(Guid deviceId) 
        => await _cameraRepository.GetByDeviceIdAsync(deviceId); 

    public async Task<Camera?> GetCameraWithDetailsAsync(int cameraId)
        => await _cameraRepository.GetCameraWithDetailsAsync(cameraId);
}
