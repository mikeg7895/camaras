using Microsoft.EntityFrameworkCore;
using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;
using Server.Infrastructure.Persistence;

namespace Server.Infrastructure.Repositories;

public class CameraRepository(ServerDbContext context) : GenericRepository<Camera>(context), ICameraRepository
{
}