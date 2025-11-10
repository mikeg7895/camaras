using Microsoft.EntityFrameworkCore;
using Server.Application.Interfaces;
using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;

namespace Server.Application.Services;

public class VideoService(IVideoRepository repository) : GenericService<Video>(repository), IVideoService
{
}