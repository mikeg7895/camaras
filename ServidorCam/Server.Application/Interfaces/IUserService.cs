using Server.Core.Domain.Entities;

namespace Server.Application.Interfaces;

public interface IUserService : IGenericService<User>
{
    Task<IEnumerable<User>> GetUsersWithApproved(bool approved);
    Task<User?> ApproveUser(int id);
    Task<IEnumerable<User>> GetAllUsersAsync();
}