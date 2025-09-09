using System;

namespace Colibri.Data.Helpers
{
    /// <summary>
    /// Вспомогательный базовый интерфейс сущностей
    /// </summary>
    public interface IAudiTable
    {
        /// <summary>
        /// Получает или устанавливает дату и время создания сущности.
        /// </summary>
        DateTime CreatedAt { get; set; }
        /// <summary>
        /// Получает или устанавливает дату и время последнего обновления сущности.
        /// </summary>
        DateTime UpdatedAt { get; set; }
    }
}
