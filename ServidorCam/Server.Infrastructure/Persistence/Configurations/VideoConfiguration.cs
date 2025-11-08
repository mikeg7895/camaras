namespace Server.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Core.Domain.Entities;

public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(v => v.FrameCount)
            .IsRequired();

        builder.Property(v => v.RecordedAt)
            .IsRequired();

        builder.Property(v => v.CameraId)
            .IsRequired();

        builder.HasOne(v => v.Camera)
            .WithMany(c => c.Videos)
            .HasForeignKey(v => v.CameraId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.ProcessedFrames)
            .WithOne(pf => pf.Video)
            .HasForeignKey(pf => pf.VideoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
