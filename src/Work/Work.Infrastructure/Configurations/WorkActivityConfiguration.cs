namespace Work.Infrastructure.Configurations;

internal sealed class WorkActivityConfiguration : IEntityTypeConfiguration<WorkActivity>
{
    public void Configure(EntityTypeBuilder<WorkActivity> builder)
    {
        builder.ToTable("WorkActivities");
        builder.HasKey(activity => activity.Id);
        builder.Property(activity => activity.Id)
            .HasConversion(id => id.Value, value => WorkActivityId.FromStorage(value))
            .HasColumnType("bigint").ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(activity => activity.UserId).IsRequired();
        builder.Property(activity => activity.UserEmail).HasMaxLength(WorkActivity.UserEmailMaxLength).IsRequired();
        builder.Property(activity => activity.Area).HasMaxLength(WorkActivity.AreaMaxLength).IsRequired();
        builder.Property(activity => activity.Operation).HasMaxLength(WorkActivity.OperationMaxLength).IsRequired();
        builder.Property(activity => activity.Method).HasMaxLength(WorkActivity.MethodMaxLength).IsRequired();
        builder.Property(activity => activity.Target).HasMaxLength(WorkActivity.TargetMaxLength).IsRequired();
        builder.Property(activity => activity.Count).IsRequired();
        builder.Property(activity => activity.FirstAt).IsRequired();
        builder.Property(activity => activity.OccurredAt).IsRequired();

        builder.HasIndex(activity => new { activity.UserId, activity.OccurredAt });
    }
}
