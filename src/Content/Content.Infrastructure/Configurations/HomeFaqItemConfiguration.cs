namespace Content.Infrastructure.Configurations;

internal sealed class HomeFaqItemConfiguration : IEntityTypeConfiguration<HomeFaqItem>
{
    public void Configure(EntityTypeBuilder<HomeFaqItem> builder)
    {
        builder.ToTable("HomeFaqItems");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).UseIdentityByDefaultColumn();
        builder.Property(item => item.QuestionUk).HasMaxLength(HomeFaqItem.QuestionMaxLength).IsRequired();
        builder.Property(item => item.QuestionRu).HasMaxLength(HomeFaqItem.QuestionMaxLength).IsRequired();
        builder.Property(item => item.AnswerUk).HasMaxLength(HomeFaqItem.AnswerMaxLength).IsRequired();
        builder.Property(item => item.AnswerRu).HasMaxLength(HomeFaqItem.AnswerMaxLength).IsRequired();
        builder.Property(item => item.CreatedAt).IsRequired();
        builder.Property(item => item.UpdatedAt).IsRequired();
        builder.HasIndex(item => item.Position, "IX_HomeFaqItems_Position");
    }
}
