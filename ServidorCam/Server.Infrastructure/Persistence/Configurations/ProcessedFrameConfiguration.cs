namespace Server.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Core.Domain.Entities;

public class ProcessedFrameConfiguration : IEntityTypeConfiguration<ProcessedFrame>
{
    public void Configure(EntityTypeBuilder<ProcessedFrame> builder)
    {
        builder.HasKey(pf => pf.Id);

        builder.Property(pf => pf.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pf => pf.FilterType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pf => pf.ProcessedAt)
            .IsRequired();

        builder.Property(pf => pf.VideoId)
            .IsRequired();

        builder.HasOne(pf => pf.Video)
            .WithMany(v => v.ProcessedFrames)
            .HasForeignKey(pf => pf.VideoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
