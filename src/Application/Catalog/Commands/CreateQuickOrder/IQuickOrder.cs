namespace Application.Catalog.Commands.CreateQuickOrder;

public interface IQuickOrder {
    string Name { get; }
    string Phone { get; }
    string? Message { get; }
}
