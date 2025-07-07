using Colibri.Data.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;

namespace Colibri.Data.Context
{
    /// <summary>
    /// Класс, представляющий пользовательский магазин (UserStore) для управления пользователями.
    /// </summary>
    public class AppUserStore : UserStore<User, Role, AppDbContext, Guid>
    {
        /// <summary>
        /// Конструктор класса AppUserStore.
        /// </summary>
        /// <param name="context">Контекст базы данных, используемый для взаимодействия с хранилищем пользователей.</param>
        /// <param name="describer">Объект для описания ошибок, возникающих при управлении пользователями.</param>
        public AppUserStore(AppDbContext context, IdentityErrorDescriber describer = null) : base(context, describer)
        {
        }
    }
}
