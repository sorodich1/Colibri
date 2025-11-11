using Colibri.Data.Helpers;

namespace Colibri.Data.Entity;

/// <summary>
/// Представляет регистрацию события, содержит информацию
/// о состоянии (активен/неактивен) для конкретного события.
/// </summary>
public class EventRegistration : AudiTable
{
    /// <summary>
    /// Внешний ключ, ссылающийся на таблицу событий.
    /// Определяет, к какому событию относится данная регистрация.
    /// </summary>
    public int EventId { get; set; }
    /// <summary>
    /// Булевое значение, указывающее, активна ли регистрация для данного события.
    /// Может использоваться для включения/отключения оповещений или мониторинга.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Навигационное свойство для связи с таблицей Event.
    /// Позволяет получать подробную информацию о событии, к которому относится регистрация.
    /// </summary>
    public Event Event { get; set; }

    public string AdditionalData { get; set; }
}
