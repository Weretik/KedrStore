using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.Domain.Runs;
using Scheduling.Domain.Tasks;

namespace Scheduling.Infrastructure.Persistence.Tasks;

public sealed class ScheduledTaskRunConfiguration : IEntityTypeConfiguration<ScheduledTaskRun>
{
    public void Configure(EntityTypeBuilder<ScheduledTaskRun> b)
    {
        b.ToTable("ScheduledTaskRuns");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.TaskKey).HasMaxLength(64);
        b.Property(x => x.Trigger).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Outcome).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.RequestedBy).HasMaxLength(200);
        b.Property(x => x.Message).HasMaxLength(ScheduledTaskErrors.MaxMessageLength);
        b.HasIndex(x => new { x.TaskKey, x.ClaimedAt }).IsDescending(false, true);
        b.HasIndex(x => x.ClaimedAt);
    }
}
