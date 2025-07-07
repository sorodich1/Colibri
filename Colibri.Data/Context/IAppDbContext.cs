using Colibri.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Colibri.Data.Context
{
    public interface IAppDbContext
    {
        /// <summary>
        /// Набор данных для логирования.
        /// </summary>
        DbSet<Log> Logger { get; set; }
        /// <summary>
        /// Набор данных для пользователей.
        /// </summary>
        DbSet<User> Users { get; set; }
        /// <summary>
        /// Набор данных для ролей.
        /// </summary>
        DbSet<Role> Roles { get; set; }
        /// <summary>
        /// Получение объекта DatabaseFacade, используемого для взаимодействия с базой данных.
        /// Позволяет выполнять операции, специфичные для базы данных, такие как транзакции и другие настройки.
        /// </summary>
        DatabaseFacade Database { get; }
        /// <summary>
        /// Получение объекта ChangeTracker, который отслеживает изменения в сущностях.
        /// Позволяет получить доступ к состоянию (добавлено, изменено, удалено) сущностей в контексте.
        /// </summary>
        ChangeTracker ChangeTracker { get; }
        /// <summary>
        /// Получение набора данных для указанной сущности.
        /// Позволяет работать с конкретной сущностью внутри контекста.
        /// </summary>
        /// <typeparam name="TEntity">Тип сущности, для которой нужно получить набор данных.</typeparam>
        /// <returns>Набор данных для указанной сущности.</returns>
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        /// <summary>
        /// Сохранение изменений, сделанных в контексте базы данных.
        /// </summary>
        /// <returns>Количество записей, измененных в базе данных.</returns>
        int SaveChanges();
    }
}
