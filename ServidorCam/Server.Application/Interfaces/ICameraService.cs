using Server.Core.Domain.Entities;

namespace Server.Application.Interfaces;

public interface ICameraService : IGenericService<Camera>
{
    Task<IEnumerable<Camera>> GetByUserIdAsync(int userId);
}