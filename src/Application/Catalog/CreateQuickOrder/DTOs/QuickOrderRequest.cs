using Application.Catalog.CreateQuickOrder.Contracts;

namespace Application.Catalog.CreateQuickOrder.DTOs;

public sealed record QuickOrderRequest(string Name, string Phone, string? Message) : IQuickOrder;
