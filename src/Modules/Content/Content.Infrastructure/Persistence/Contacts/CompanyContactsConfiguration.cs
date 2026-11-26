namespace Content.Infrastructure.Persistence.Contacts;

internal sealed class CompanyContactsConfiguration : IEntityTypeConfiguration<CompanyContacts>
{
    public void Configure(EntityTypeBuilder<CompanyContacts> builder)
    {
        builder.ToTable("CompanyContacts", table => table.HasCheckConstraint(
            "CK_CompanyContacts_Singleton", $"\"Id\" = {CompanyContacts.SingletonId}"));
        builder.HasKey(contacts => contacts.Id);
        builder.Property(contacts => contacts.Id)
            .HasConversion(id => id.Value, value => CompanyContactsId.FromStorage(value)).ValueGeneratedNever();
        builder.Property(contacts => contacts.SalesPhone).HasMaxLength(13).IsRequired();
        builder.Property(contacts => contacts.Email).HasMaxLength(CompanyContacts.EmailMaxLength).IsRequired();
        builder.Property(contacts => contacts.TelegramUrl).HasMaxLength(CompanyContacts.UrlMaxLength).IsRequired();
        builder.Property(contacts => contacts.ViberPhone).HasMaxLength(13).IsRequired();
        builder.Property(contacts => contacts.MapUrl).HasMaxLength(CompanyContacts.UrlMaxLength).IsRequired();
        builder.Property(contacts => contacts.CityUk).HasMaxLength(CompanyContacts.CityMaxLength).IsRequired();
        builder.Property(contacts => contacts.CityRu).HasMaxLength(CompanyContacts.CityMaxLength).IsRequired();
        builder.OwnsOne(contacts => contacts.Store, store =>
        {
            store.Property(item => item.StoreName).HasColumnName("StoreName").HasMaxLength(CompanyStore.NameMaxLength);
            store.Property(item => item.StreetAddressUk).HasColumnName("StreetAddressUk").HasMaxLength(CompanyStore.StreetMaxLength);
            store.Property(item => item.StreetAddressRu).HasColumnName("StreetAddressRu").HasMaxLength(CompanyStore.StreetMaxLength);
            store.Property(item => item.RegionUk).HasColumnName("RegionUk").HasMaxLength(CompanyStore.RegionMaxLength);
            store.Property(item => item.RegionRu).HasColumnName("RegionRu").HasMaxLength(CompanyStore.RegionMaxLength);
            store.Property(item => item.PostalCode).HasColumnName("PostalCode").HasMaxLength(5);
            store.Property(item => item.WeekdaysOpens).HasColumnName("WeekdaysOpens");
            store.Property(item => item.WeekdaysCloses).HasColumnName("WeekdaysCloses");
            store.Property(item => item.SaturdayOpens).HasColumnName("SaturdayOpens");
            store.Property(item => item.SaturdayCloses).HasColumnName("SaturdayCloses");
            store.Property(item => item.SundayOpens).HasColumnName("SundayOpens");
            store.Property(item => item.SundayCloses).HasColumnName("SundayCloses");
        });
        builder.Navigation(contacts => contacts.Store).IsRequired();
        builder.Property(contacts => contacts.UpdatedAt).IsRequired();
        builder.Property(contacts => contacts.UpdatedBy).HasMaxLength(EditorName.MaxLength);
    }
}
