using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Outbox;

public static class OutboxModelBuilderExtensions
{
    public const string TableName = "OutboxMessages";

    /// <summary>Maps the module's outbox table into its own schema (platform spec 006, R-001).</summary>
    public static ModelBuilder ApplyOutbox(this ModelBuilder modelBuilder, string schema)
    {
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable(TableName, schema);
            builder.HasKey(message => message.Id);
            builder.Property(message => message.Id).ValueGeneratedNever();
            builder.Property(message => message.Type).HasMaxLength(200).IsRequired();
            builder.Property(message => message.Payload).HasColumnType("jsonb").IsRequired();
            builder.Property(message => message.LastError).HasMaxLength(OutboxMessage.MaximumErrorLength);
            builder.HasIndex(message => message.NextAttemptAtUtc)
                .HasFilter("\"ProcessedAtUtc\" IS NULL AND \"FailedAtUtc\" IS NULL")
                .HasDatabaseName("IX_OutboxMessages_Due");
        });

        return modelBuilder;
    }
}
