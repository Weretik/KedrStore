﻿namespace Infrastructure.Common.Configurations;

public static class CategoryMapping
{
    public static void Configure<TId, TNode>(
        EntityTypeBuilder<TNode> builder,
        string tableName,
        int nameMaxLen = 100)
        where TNode : BaseCategory<TId, TNode>
        where TId   : struct
    {
        builder.ToTable(tableName);

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .HasMaxLength(nameMaxLen)
            .IsRequired();

        builder.Property(c => c.Path)
            .HasColumnType("ltree")
            .IsRequired();

        builder.HasIndex(c => c.Path)
            .HasMethod("gist")
            .HasDatabaseName($"IX_{tableName}_Path_gist");
    }
}
