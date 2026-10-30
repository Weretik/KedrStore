using Catalog.Domain.Entities;
using Catalog.Infrastructure.Converters;

namespace Catalog.Infrastructure.Configurations;

public sealed class CategoryContentConfiguration : IEntityTypeConfiguration<CategoryContent>
{
    public void Configure(EntityTypeBuilder<CategoryContent> builder)
    {
        builder.ToTable("CategoryContents");

        builder.HasKey(x => new { x.CategoryId, x.Language });

        builder.Property(x => x.CategoryId)
            .HasConversion(CatalogConverter.ProductCategoryIdConvert)
            .ValueGeneratedNever();

        builder.Property(x => x.Language)
            .HasMaxLength(2)
            .IsFixedLength()
            .IsRequired();

        builder.Property(x => x.SeoTitle).HasMaxLength(CategoryContent.SeoTitleMaxLength);
        builder.Property(x => x.MetaDescription).HasMaxLength(CategoryContent.MetaDescriptionMaxLength);
        builder.Property(x => x.H1).HasMaxLength(CategoryContent.H1MaxLength);
        builder.Property(x => x.BodyHtml).HasMaxLength(CategoryContent.BodyHtmlMaxLength);
        builder.Property(x => x.UpdatedBy).HasMaxLength(CategoryContent.UpdatedByMaxLength).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne<ProductCategory>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(x => x.Faq, faq =>
        {
            faq.ToTable("CategoryFaqItems");
            faq.WithOwner().HasForeignKey("CategoryId", "Language");
            faq.Property<string>("Language").HasMaxLength(2).IsFixedLength();
            faq.HasKey(x => x.Id);
            faq.Property(x => x.Id).UseIdentityByDefaultColumn();
            faq.Property(x => x.SortOrder).IsRequired();
            faq.Property(x => x.Question).HasMaxLength(CategoryFaqItem.QuestionMaxLength).IsRequired();
            faq.Property(x => x.Answer).HasMaxLength(CategoryFaqItem.AnswerMaxLength).IsRequired();
            faq.HasIndex("CategoryId", "Language", nameof(CategoryFaqItem.SortOrder))
                .HasDatabaseName("IX_CategoryFaqItems_Content_SortOrder");
        });

        builder.Navigation(x => x.Faq).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
