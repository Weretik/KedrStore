namespace Content.Infrastructure.Persistence.Wholesale;

/// <summary>One row; each language is one jsonb document, read and replaced whole.</summary>
internal sealed class WholesalePageConfiguration : IEntityTypeConfiguration<WholesalePage>
{
    public void Configure(EntityTypeBuilder<WholesalePage> builder)
    {
        builder.ToTable("WholesalePages", table => table.HasCheckConstraint(
            "CK_WholesalePages_Singleton", $"\"Id\" = {WholesalePage.SingletonId}"));
        builder.HasKey(page => page.Id);
        builder.Property(page => page.Id).ValueGeneratedNever();
        builder.Property(page => page.UpdatedAt).IsRequired();
        builder.Property(page => page.UpdatedBy).HasMaxLength(EditorName.MaxLength);
        builder.OwnsOne(page => page.Uk, text => Text(text, "Uk"));
        builder.OwnsOne(page => page.Ru, text => Text(text, "Ru"));
    }

    private static void Text(OwnedNavigationBuilder<WholesalePage, WholesalePageText> text, string column)
    {
        text.ToJson(column);
        text.OwnsMany(value => value.Steps);
        text.OwnsMany(value => value.Faq);
        text.Navigation(value => value.Steps).HasField("_steps").UsePropertyAccessMode(PropertyAccessMode.Field);
        text.Navigation(value => value.Faq).HasField("_faq").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
