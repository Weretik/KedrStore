using BuildingBlocks.Domain.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Maps the shared-kernel ids (ADR-0017) in a module's DbContext: each one is stored as its plain value, so a key or a
/// reference to another module's aggregate needs no converter of its own and the schema does not change.
/// </summary>
public static class KernelIdConventions
{
    public static ModelConfigurationBuilder AddKernelIds(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<UserId>().HaveConversion<UserIdConverter>();
        configurationBuilder.Properties<MediaFileId>().HaveConversion<MediaFileIdConverter>();
        configurationBuilder.Properties<ProductId>().HaveConversion<ProductIdConverter>();
        configurationBuilder.Properties<ProductCategoryId>().HaveConversion<ProductCategoryIdConverter>();
        configurationBuilder.Properties<PriceTypeId>().HaveConversion<PriceTypeIdConverter>();
        return configurationBuilder;
    }

    private sealed class UserIdConverter() : ValueConverter<UserId, Guid>(id => id.Value, value => UserId.FromStorage(value));

    private sealed class MediaFileIdConverter()
        : ValueConverter<MediaFileId, long>(id => id.Value, value => MediaFileId.FromStorage(value));

    private sealed class ProductIdConverter()
        : ValueConverter<ProductId, int>(id => id.Value, value => ProductId.FromStorage(value));

    private sealed class ProductCategoryIdConverter()
        : ValueConverter<ProductCategoryId, int>(id => id.Value, value => ProductCategoryId.FromStorage(value));

    private sealed class PriceTypeIdConverter()
        : ValueConverter<PriceTypeId, int>(id => id.Value, value => PriceTypeId.FromStorage(value));
}
