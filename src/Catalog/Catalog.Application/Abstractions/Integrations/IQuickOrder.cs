using Catalog.Application.Features.Orders.Commands.CreateOrder;
namespace Catalog.Application.Abstractions.Integrations;

public interface IQuickOrder {
    string Name { get; }
    string Phone { get; }
    string? Message { get; }
}
