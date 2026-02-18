namespace Catalog.Application.Features.Orders.Create.Contracts;

public interface IQuickOrder {
    string Name { get; }
    string Phone { get; }
    string? Message { get; }
}
