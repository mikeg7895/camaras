using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Core.Domain.Entities;

namespace Server.Core.Domain.Interfaces;

public interface ICameraRepository : IGenericRepository<Camera>
{
    Task<IEnumerable<Camera>> GetByUserIdAsync(int userId);
    Task<Camera?> GetByDeviceIdAsync(Guid deviceId);
    Task<Camera?> GetCameraWithDetailsAsync(int cameraId);
}
