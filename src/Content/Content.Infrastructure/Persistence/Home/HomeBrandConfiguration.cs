namespace Content.Infrastructure.Persistence.Home;

internal sealed class HomeBrandConfiguration : IEntityTypeConfiguration<HomeBrand>
{
    public void Configure(EntityTypeBuilder<HomeBrand> builder)
    {
        builder.ToTable("HomeBrands");
        builder.HasKey(brand => brand.Id);
        builder.Property(brand => brand.Id).UseIdentityByDefaultColumn();
        builder.Property(brand => brand.Name).HasMaxLength(HomeBrand.NameMaxLength).IsRequired();
        // A media file ID is a value from the Media module, not a foreign key (module rules).
        builder.Property(brand => brand.LogoMediaFileId).IsRequired();
        builder.Property(brand => brand.CreatedAt).IsRequired();
        builder.Property(brand => brand.UpdatedAt).IsRequired();
        builder.HasIndex(brand => brand.Position, "IX_HomeBrands_Position");
    }
}
