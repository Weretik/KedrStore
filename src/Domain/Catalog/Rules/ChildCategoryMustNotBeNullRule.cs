namespace Domain.Catalog.Rules;

public sealed class ChildCategoryMustNotBeNullRule(Category? child)
    : IBusinessRule
{
    public string Message => "Категорія дитини не повинна бути нульовою.";

    public bool IsBroken() => child is null;
}
