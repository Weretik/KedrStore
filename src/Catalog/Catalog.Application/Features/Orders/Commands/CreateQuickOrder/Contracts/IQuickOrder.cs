namespace Catalog.Application.Features.Orders.Commands.CreateQuickOrder.Contracts;

public interface IQuickOrder {
    string Name { get; }
    string Phone { get; }
    string? Message { get; }
}
