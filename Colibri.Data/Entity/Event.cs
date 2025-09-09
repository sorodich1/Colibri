using System.Collections.Generic;
using Colibri.Data.Helpers;

namespace Colibri.Data.Entity;

/// <summary>
/// Представляет событие, содержащее основные характеристики и связанные регистрации.
/// </summary>
public class Event : AudiTable
{
    /// <summary>
    /// Название события.
    /// Например, "Обновление системы" или "Техническое обслуживание".
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Описание события.
    /// Может содержать дополнительную информацию или детали.
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// Тип события.
    /// Например, "Системное", "Плановое", "Аварийное" и т.д.
    /// Позволяет классифицировать событие по категориям.
    /// </summary>
    public string EventType { get; set; }
    /// <summary>
    /// Коллекция регистраций, связанных с этим событием.
    /// Используется для отслеживания всех регистраций, связанных с данным событием.
    /// </summary>
    public ICollection<EventRegistration> Registrations { get; set; }
}
