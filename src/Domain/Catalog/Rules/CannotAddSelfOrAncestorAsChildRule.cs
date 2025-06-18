namespace Domain.Catalog.Rules;

public sealed class CannotAddSelfOrAncestorAsChildRule(Category self, Category child)
    : IBusinessRule
{
    public string Message => "Неможливо додати дочірню категорію: буде створено циклічне посилання.";

    public bool IsBroken()
    {
        if (child.Id == self.Id)
            return true;

        return child.IsDescendantOf(self.Id);
    }
}
