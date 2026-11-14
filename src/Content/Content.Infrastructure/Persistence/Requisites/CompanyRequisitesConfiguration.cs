namespace Content.Infrastructure.Persistence.Requisites;

internal sealed class CompanyRequisitesConfiguration : IEntityTypeConfiguration<CompanyRequisites>
{
    public void Configure(EntityTypeBuilder<CompanyRequisites> builder)
    {
        builder.ToTable("CompanyRequisites", table => table.HasCheckConstraint(
            "CK_CompanyRequisites_Singleton", $"\"Id\" = {CompanyRequisites.SingletonId}"));
        builder.HasKey(requisites => requisites.Id);
        builder.Property(requisites => requisites.Id).ValueGeneratedNever();
        builder.Property(requisites => requisites.LegalName)
            .HasMaxLength(CompanyRequisites.LegalNameMaxLength).IsRequired();
        builder.Property(requisites => requisites.Code).HasMaxLength(10).IsRequired();
        builder.Property(requisites => requisites.RegistrationExtractNumber)
            .HasMaxLength(CompanyRequisites.ExtractNumberMaxLength).IsRequired();
        builder.Property(requisites => requisites.Iban).HasMaxLength(29).IsRequired();
        builder.Property(requisites => requisites.BankName)
            .HasMaxLength(CompanyRequisites.BankNameMaxLength).IsRequired();
        builder.Property(requisites => requisites.BankCode).HasMaxLength(6).IsRequired();
        builder.Property(requisites => requisites.UpdatedAt).IsRequired();
    }
}
