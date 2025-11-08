using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;
using Server.Infrastructure.Persistence;

namespace Server.Infrastructure.Repositories;

public class VideoRepository(ServerDbContext context) : GenericRepository<Video>(context), IVideoRepository
{
}