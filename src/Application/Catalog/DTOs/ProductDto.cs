namespace Application.Catalog.DTOs;

public record ProductDto : IMapWith<Product>
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Manufacturer { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public int CategoryId { get; init; }
    public string? Photo { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId.Value))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency));
    }
}
