namespace Work.Infrastructure.Configurations;

internal sealed class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.ToTable("WorkTasks");
        builder.HasKey(task => task.Id);
        builder.Property(task => task.Id)
            .HasConversion(id => id.Value, value => WorkTaskId.FromStorage(value))
            .HasColumnType("bigint").ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(task => task.OwnerUserId).IsRequired();
        builder.Property(task => task.CreatedByUserId).IsRequired();
        builder.Property(task => task.Title).HasMaxLength(WorkTask.TitleMaxLength).IsRequired();
        builder.Property(task => task.Details).HasMaxLength(WorkTask.DetailsMaxLength);
        builder.Property(task => task.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(task => task.Result).HasMaxLength(WorkTask.ResultMaxLength);
        builder.Property(task => task.CreatedAt).IsRequired();
        builder.Property(task => task.UpdatedAt).IsRequired();
        builder.Property(task => task.DueDate).HasColumnType("date");
        builder.Ignore(task => task.IsCreatedByOwner);

        // Steps are read and saved only with their task, so they live in one jsonb column.
        builder.OwnsMany(task => task.Steps, steps =>
        {
            steps.ToJson("Steps");
            steps.Property(step => step.Title).IsRequired();
            steps.Property(step => step.IsDone).IsRequired();
        });
        builder.Navigation(task => task.Steps)
            .HasField("_steps").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(task => new { task.OwnerUserId, task.Status });
        builder.HasIndex(task => new { task.OwnerUserId, task.CompletedAt });
    }
}
