namespace Infrastructure.Catalog.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new CategoryId(value));
                //.HasColumnName("CategoryId")
                //.HasColumnType("int");

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ParentCategoryId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (int?)null,
                value => value.HasValue ?  new CategoryId(value.Value) : null);

        // Настройка self-referencing relationship для иерархии категорий
        builder.HasMany(c => c.Children)
            .WithOne()
            .HasForeignKey(c => c.ParentCategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Индекс для оптимизации поиска дочерних категорий
        builder.HasIndex(x => x.ParentCategoryId);

        // Настройка фильтра для мягкого удаления
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
