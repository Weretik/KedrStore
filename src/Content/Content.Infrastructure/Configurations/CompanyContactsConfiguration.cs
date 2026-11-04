namespace Content.Infrastructure.Configurations;

internal sealed class CompanyContactsConfiguration : IEntityTypeConfiguration<CompanyContacts>
{
    public void Configure(EntityTypeBuilder<CompanyContacts> builder)
    {
        builder.ToTable("CompanyContacts", table => table.HasCheckConstraint(
            "CK_CompanyContacts_Singleton", $"\"Id\" = {CompanyContacts.SingletonId}"));
        builder.HasKey(contacts => contacts.Id);
        builder.Property(contacts => contacts.Id).ValueGeneratedNever();
        builder.Property(contacts => contacts.SalesPhone).HasMaxLength(13).IsRequired();
        builder.Property(contacts => contacts.Email).HasMaxLength(CompanyContacts.EmailMaxLength).IsRequired();
        builder.Property(contacts => contacts.TelegramUrl).HasMaxLength(CompanyContacts.UrlMaxLength).IsRequired();
        builder.Property(contacts => contacts.ViberPhone).HasMaxLength(13).IsRequired();
        builder.Property(contacts => contacts.MapUrl).HasMaxLength(CompanyContacts.UrlMaxLength).IsRequired();
        builder.Property(contacts => contacts.CityUk).HasMaxLength(CompanyContacts.CityMaxLength).IsRequired();
        builder.Property(contacts => contacts.CityRu).HasMaxLength(CompanyContacts.CityMaxLength).IsRequired();
        builder.Property(contacts => contacts.UpdatedAt).IsRequired();
    }
}
