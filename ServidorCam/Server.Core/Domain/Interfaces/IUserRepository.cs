using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Core.Domain.Entities;

namespace Server.Core.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetByApprovalStatusAsync(bool approved);
    Task<IEnumerable<User>> GetAllWithCamerasAsync();
}
