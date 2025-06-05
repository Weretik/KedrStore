namespace Application.Common.Exceptions
{
    /// <summary>
    /// Исключение, которое выбрасывается, когда запрашиваемый ресурс не найден
    /// </summary>
    public class NotFoundException(string entityName, object key)
        : UseCaseException($"{entityName} with key '{key}' was not found.");
}
