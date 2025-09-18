namespace Presentation.Shared.ViewModels;

public sealed class QuickOrderVm(string name = "", string phone = "", string? message = null) : IQuickOrder
{
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public string? Message { get; set; } = message;
}
