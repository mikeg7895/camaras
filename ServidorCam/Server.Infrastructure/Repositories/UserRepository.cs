using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;
using Server.Infrastructure.Persistence;

namespace Server.Infrastructure.Repositories;

public class UserRepository(ServerDbContext context) : GenericRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetByApprovalStatusAsync(bool approved)
    {
        return await _context.Users
            .Where(u => u.Approved == approved)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetAllWithCamerasAsync()
    {
        return await _context.Users
            .Include(u => u.Cameras)
            .ToListAsync();
    }
}
