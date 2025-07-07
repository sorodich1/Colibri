using System;
using System.Text;

namespace Colibri.Data.Helpers
{
    /// <summary>
    /// Вспомогательный класс с методами для обработки исключений.
    /// </summary>
    public class Auxiliary
    {
        /// <summary>
        /// Получает детализированное сообщение об исключении, включая информацию о внутреннем исключении.
        /// </summary>
        /// <param name="ex">Исключение, для которого требуется получить подробное сообщение.</param>
        /// <returns>Строка с детализированным сообщением об исключении.</returns>
        /// <exception cref="ArgumentNullException">Выдается, если переданное исключение равно null.</exception>
        public static string GetDetailedExceptionMessage(Exception ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex), "Исключение отсутствует");
            }

            StringBuilder sb = new();

            sb.AppendLine($"Тип ошибки: {ex.GetType()}");
            sb.AppendLine($"Сообщение: {ex.Message}");
            sb.AppendLine($"Трассировка стека: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                sb.AppendLine($"Внутреннее исключение: ");
                sb.AppendLine(GetDetailedExceptionMessage(ex.InnerException));
            }
            return sb.ToString();
        }
    }
}
