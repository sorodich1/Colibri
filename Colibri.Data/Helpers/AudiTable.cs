using System;

namespace Colibri.Data.Helpers
{
    public class AudiTable
    {
        /// <summary>
        /// Дата создания поля
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Дата изменения поля
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        /// <summary>
        /// Отметка об удалении
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}
