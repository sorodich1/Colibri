using System;

namespace Colibri.Data.Helpers
{
    /// <summary>
    /// Расширение для строк
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Метод преобразования строковых переменных в Guid
        /// </summary>
        /// <param name="str">Принемаемая строка</param>
        /// <returns>Guid</returns>
        /// <exception cref="ArgumentException">Возвращается исключение если значение строки равно null</exception>
        /// <exception cref="FormatException">Возвращается исключение если формат строки не соответствует Guid</exception>
        public static Guid ToGuid(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Строка не может быть пустой или null.", nameof(str));
            }

            if (Guid.TryParse(str, out Guid guid))
            {
                return guid;
            }
            else
            {
                throw new FormatException("Невозможно преобразовать строку в Guid.");
            }
        }
    }
}
