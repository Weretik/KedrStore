using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Scheduling.Domain.Runs;
using Scheduling.Domain.Schedules;
using Scheduling.Domain.Tasks;

namespace Scheduling.Infrastructure.Persistence.Tasks;

public sealed class ScheduledTaskConfiguration : IEntityTypeConfiguration<ScheduledTask>
{
    public void Configure(EntityTypeBuilder<ScheduledTask> b)
    {
        b.ToTable("ScheduledTasks");
        b.HasKey(x => x.Key);
        b.Property(x => x.Key).HasMaxLength(64)
            .HasConversion(x => x.Value, x => ScheduledTaskKey.FromStorage(x));
        b.Property(x => x.Schedule).HasColumnType("jsonb")
            .HasConversion(x => ScheduleDocument.Write(x), x => ScheduleDocument.Read(x),
                new ValueComparer<TaskSchedule>((a, c) => a!.Equals(c), x => x.GetHashCode(), x => x));
        b.Property(x => x.CurrentRunId)
            .HasConversion(new ValueConverter<ScheduledTaskRunId, Guid>(x => x.Value, x => ScheduledTaskRunId.FromStorage(x)));
        b.Property(x => x.State).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.RunRequestedBy).HasMaxLength(200);
        b.Property(x => x.UpdatedBy).HasMaxLength(200);
        b.Property(x => x.LastSuccessSummary).HasMaxLength(ScheduledTaskErrors.MaxMessageLength);
        b.Property(x => x.LastFailureReason).HasMaxLength(ScheduledTaskErrors.MaxMessageLength);
        b.Property(x => x.Version).IsConcurrencyToken();
    }
}
