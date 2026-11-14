namespace Content.Infrastructure.Persistence.Requisites;

internal sealed class CompanyRequisitesChangeConfiguration : IEntityTypeConfiguration<CompanyRequisitesChange>
{
    public void Configure(EntityTypeBuilder<CompanyRequisitesChange> builder)
    {
        builder.ToTable("CompanyRequisitesChanges");
        builder.HasKey(change => change.Id);
        builder.Property(change => change.Id).UseIdentityAlwaysColumn();
        builder.Property(change => change.ChangedAt).IsRequired();
        builder.Property(change => change.ChangedByUserId).HasMaxLength(64).IsRequired();
        builder.Property(change => change.ChangedByName)
            .HasMaxLength(CompanyRequisitesChange.UserNameMaxLength).IsRequired();
        builder.HasIndex(change => change.ChangedAt);

        // The changed fields are read only together with their entry, so they live in one jsonb column.
        builder.OwnsMany(change => change.Changes, changes =>
        {
            changes.ToJson("Changes");
            changes.Property(item => item.Field).IsRequired();
            changes.Property(item => item.OldValue).IsRequired();
            changes.Property(item => item.NewValue).IsRequired();
        });
        builder.Navigation(change => change.Changes)
            .HasField("_changes").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
