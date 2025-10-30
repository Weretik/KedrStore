namespace Application.Catalog.CreateQuickOrder.Contracts;

public interface IQuickOrder {
    string Name { get; }
    string Phone { get; }
    string? Message { get; }
}
