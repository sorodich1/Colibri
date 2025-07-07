using Microsoft.AspNetCore.Identity;
using System;

namespace Colibri.Data.Entity
{
    /// <summary>
    /// Сущность роли пользователя
    /// </summary>
    public class Role : IdentityRole<Guid>
    {
        /// <summary>
        /// Дата создания роли
        /// </summary>
        public DateTime LastRole { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Описание роли
        /// </summary>
        public string Description { get; set; }
    }
}
