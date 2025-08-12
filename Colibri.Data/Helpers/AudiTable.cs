using System;

namespace Colibri.Data.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class AudiTable
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
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
