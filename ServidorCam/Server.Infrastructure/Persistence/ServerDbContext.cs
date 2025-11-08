namespace Server.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Server.Core.Domain.Entities;

public class ServerDbContext : DbContext
{
    public ServerDbContext(DbContextOptions<ServerDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Camera> Cameras { get; set; }
    public DbSet<Video> Videos { get; set; }
    public DbSet<ProcessedFrame> ProcessedFrames { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}