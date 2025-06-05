using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Common.Validation
{
    /// <summary>
    /// Предоставляет методы для проверки условий и выброса исключений при их невыполнении
    /// </summary>
    public static class Ensure
    {
        /// <summary>
        /// Проверяет, что значение не null
        /// </summary>
        /// <param name="value">Проверяемое значение</param>
        /// <param name="parameterName">Имя параметра для сообщения об ошибке</param>
        /// <exception cref="ArgumentNullException">Если значение null</exception>
        public static T NotNull<T>(T value, string parameterName)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);

            return value;
        }

        /// <summary>
        /// Проверяет, что строка не пустая и не содержит только пробелы
        /// </summary>
        /// <exception cref="ArgumentException">Если строка пустая или содержит только пробелы</exception>
        public static string NotNullOrWhiteSpace(string value, string parameterName)
        {
            NotNull(value, parameterName);

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Значение не может быть пустым или содержать только пробелы", parameterName);

            return value;
        }

        /// <summary>
        /// Проверяет, что коллекция не пуста
        /// </summary>
        /// <exception cref="ArgumentException">Если коллекция пуста</exception>
        public static IEnumerable<T> NotEmpty<T>(IEnumerable<T> collection, string parameterName)
        {
            NotNull(collection, parameterName);

            if (!collection.Any())
                throw new ArgumentException("Коллекция не может быть пустой", parameterName);

            return collection;
        }

        /// <summary>
        /// Проверяет, что условие истинно
        /// </summary>
        /// <exception cref="ArgumentException">Если условие ложно</exception>
        public static void That(bool condition, string parameterName, string message)
        {
            if (!condition)
                throw new ArgumentException(message, parameterName);
        }
    }
}
