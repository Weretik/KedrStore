namespace Domain.Catalog.Rules;

public class PhotoMustNotBeEmptyRule(string photo)
    : BusinessRule
{
    public override string Message => "Фото не може бути порожнім.";
    public override bool IsBroken() => string.IsNullOrWhiteSpace(photo);
}
