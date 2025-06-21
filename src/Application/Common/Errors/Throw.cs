namespace Application.Common.Errors;

public static class Throw
{
    public static void Application(AppError error)
    {
        throw new AppException(error.Code, BuildMessage(error));
    }

    public static void Application(AppError error, string details)
    {
        throw new AppException(error.Code, $"{error.Message} | {details}");
    }

    public static void Application(AppError error, Exception inner)
    {
        throw new AppException(error.Code, BuildMessage(error), inner);
    }

    private static string BuildMessage(AppError error)
    {
        return error.Details is not null ? $"{error.Message} | {error.Details}" : error.Message;
    }
}
