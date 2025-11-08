using Server.Core.Domain.Entities;

namespace Server.Core.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}