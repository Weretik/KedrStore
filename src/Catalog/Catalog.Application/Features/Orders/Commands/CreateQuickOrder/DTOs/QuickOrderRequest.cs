using Catalog.Application.Features.Orders.Commands.CreateQuickOrder.Contracts;

namespace Catalog.Application.Features.Orders.Commands.CreateQuickOrder.DTOs;

public sealed record QuickOrderRequest(string Name, string Phone, string? Message) : IQuickOrder;
