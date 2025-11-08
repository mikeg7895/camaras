using Microsoft.EntityFrameworkCore;
using Server.Application.Interfaces;
using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;

namespace Server.Application.Services;

public class UserService(IUserRepository repository) : GenericService<User>(repository), IUserService
{
    public async Task<IEnumerable<User>> GetUsersWithApproved(bool approved)
    {
        return await ((IUserRepository)_repository)
            .GetAll()
            .Where(u => u.Approved == approved)
            .ToListAsync();
    }

    public async Task<User?> ApproveUser(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user != null)
        {
            user.Approved = true;
            _repository.Update(user);
            await _repository.SaveChangesAsync();
            return user;
        }
        return null;
    }
}