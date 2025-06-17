namespace Application.Common.Exceptions
{
    public class NotFoundException(string entityName, object key)
        : UseCaseException($"{entityName} with key '{key}' was not found.");
}
