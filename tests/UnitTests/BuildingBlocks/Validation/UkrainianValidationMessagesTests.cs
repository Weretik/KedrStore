using BuildingBlocks.Application.Validation;
using FluentValidation;

namespace UnitTests.BuildingBlocks.Validation;

public sealed class UkrainianValidationMessagesTests
{
    private sealed record Input(string? Name, string? Email, int Count, DayOfWeek Day, string? Code);

    private sealed class InputValidator : AbstractValidator<Input>
    {
        public InputValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(5);
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.Count).InclusiveBetween(1, 10);
            RuleFor(x => x.Day).IsInEnum();
            RuleFor(x => x.Code).Matches("^[A-Z]+$");
        }
    }

    [Fact]
    public void BuiltInMessages_AreUkrainianAndDoNotRepeatTheFieldName()
    {
        UkrainianValidationMessages.Use();

        var errors = new InputValidator()
            .Validate(new Input("", "not-an-email", 0, (DayOfWeek)42, "abc"))
            .Errors.ToDictionary(error => error.PropertyName, error => error.ErrorMessage);

        Assert.Equal("Заповніть це поле.", errors["Name"]);
        Assert.Equal("Введіть коректну електронну пошту.", errors["Email"]);
        Assert.Equal("Значення має бути від 1 до 10.", errors["Count"]);
        Assert.Equal("Виберіть одне з дозволених значень.", errors["Day"]);
        Assert.Equal("Значення має невірний формат.", errors["Code"]);
        Assert.All(errors, error => Assert.DoesNotContain(error.Key, error.Value));
    }

    [Fact]
    public void MaximumLength_NamesTheLimitAndTheCurrentLength()
    {
        UkrainianValidationMessages.Use();

        var error = Assert.Single(new InputValidator().Validate(new Input("Довга назва", null, 1, DayOfWeek.Monday, null)).Errors);

        Assert.Equal("Не більше 5 символів (зараз 11).", error.ErrorMessage);
    }

    [Fact]
    public void ValidationBehavior_TurnsTheUkrainianMessagesOn()
    {
        _ = new global::BuildingBlocks.Application.Behaviors.ValidationBehavior<DummyCommand, Ardalis.Result.Result>(
            [], Microsoft.Extensions.Logging.Abstractions.NullLogger<
                global::BuildingBlocks.Application.Behaviors.ValidationBehavior<DummyCommand, Ardalis.Result.Result>>.Instance);

        Assert.IsType<UkrainianValidationMessages>(ValidatorOptions.Global.LanguageManager);
    }

    public sealed record DummyCommand : Mediator.ICommand<Ardalis.Result.Result>;
}
