namespace Application.Catalog.Commands.CreateQuickOrder;

public sealed record QuickOrderRequest(string Name, string Phone, string? Message) : IQuickOrder;
