namespace Application.Common.Validation
{
    /// <summary>
    /// Предоставляет методы для проверки условий и выброса исключений при их невыполнении
    /// </summary>
    public static class Ensure
    {
        public static T NotNull<T>(T value, string parameterName)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);

            return value;
        }
        public static string NotNullOrWhiteSpace(string value, string parameterName)
        {
            NotNull(value, parameterName);

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Значение не может быть пустым или содержать только пробелы", parameterName);

            return value;
        }
        public static IEnumerable<T> NotEmpty<T>(IEnumerable<T> collection, string parameterName)
        {
            NotNull(collection, parameterName);

            if (!collection.Any())
                throw new ArgumentException("Коллекция не может быть пустой", parameterName);

            return collection;
        }
        public static void That(bool condition, string parameterName, string message)
        {
            if (!condition)
                throw new ArgumentException(message, parameterName);
        }
    }
}
