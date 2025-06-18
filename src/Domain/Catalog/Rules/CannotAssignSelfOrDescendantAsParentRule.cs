namespace Domain.Catalog.Rules;

public sealed class CannotAssignSelfOrDescendantAsParentRule(Category self, CategoryId newParentId)
    : IBusinessRule
{
    public string Message => "Неможливо призначити батьківську категорію: буде створено циклічне посилання.";

    public bool IsBroken()
    {
        if (newParentId == self.Id)
            return true;

        return self.IsDescendantOf(newParentId);
    }
}
