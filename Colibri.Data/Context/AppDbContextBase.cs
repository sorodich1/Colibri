using Colibri.Data.Entity;
using Colibri.Data.Helpers;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Colibri.Data.Context
{
    /// <summary>
    /// Конструктор контекста данных.
    /// </summary>
    /// <param name="options">Параметры конфигурации базы данных.</param>
    public class AppDbContextBase(DbContextOptions options) : IdentityDbContext<User, Role, Guid>(options)
    {
        private const string DefaultUserName = "Anonymus"; //имя пользователя по умолчанию.

        /// <summary>
        ///  Результат сохранения изменений.
        /// </summary>
        public SaveChangeResult SaveChangeResult { get; } = new SaveChangeResult();
        /// <summary>
        /// Настройка моделей данных.
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.Email).IsRequired(false);
            });
        }
        /// <summary>
        /// Асинхронное сохранение изменений с опцией для подтверждения всех изменений.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">Если true, принимает все изменения при успешном сохранении.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Количество измененных записей в базе данных.</returns>
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            try
            {
                DbSaveChange();
                return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            catch (Exception ex)
            {
                SaveChangeResult.Exception = ex;
                return Task.FromResult(0);
            }
        }
        /// <summary>
        /// Синхронное сохранение изменений.
        /// </summary>
        /// <returns>Количество измененных записей в базе данных.</returns>
        public override int SaveChanges()
        {
            try
            {
                DbSaveChange();
                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                SaveChangeResult.Exception = ex;
                return 0;
            }
        }
        /// <summary>
        /// Синхронное сохранение изменений с опцией для подтверждения всех изменений.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">Если true, принимает все изменения при успешном сохранении.</param>
        /// <returns>Количество измененных записей в базе данных.</returns>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            try
            {
                DbSaveChange();
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }
            catch (Exception ex)
            {
                SaveChangeResult.Exception = ex;
                return 0;
            }
        }
        /// <summary>
        /// Асинхронное сохранение изменений.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Количество измененных записей в базе данных.</returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                DbSaveChange();
                return base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                SaveChangeResult.Exception = ex;
                return Task.FromResult(0);
            }
        }
        /// <summary>
        /// Обработка изменений перед их сохранением в базу данных.
        /// </summary>
        private void DbSaveChange()
        {
            var createdEntries = ChangeTracker.Entries().Where(e => e.State == EntityState.Added);

            foreach (var entry in createdEntries)
            {
                if (!(entry.Entity is IAudiTable))
                {
                    continue;
                }

                var creationDate = DateTime.UtcNow.ToUniversalTime();

                //var userName = entry.Property("CreatedBy").CurrentValue == null
                //    ? DefaultUserName : entry.Property("CreatedBy").CurrentValue;

                var updateAt = entry.Property("UpdatedAt").CurrentValue;

                var createdAt = entry.Property("CreatedAt").CurrentValue;

                if (createdAt != null)
                {
                    if (DateTime.Parse(createdAt.ToString()).Year > 1970)
                    {
                        entry.Property("CreatedAt").CurrentValue = ((DateTime)createdAt).ToUniversalTime();
                    }
                    else
                    {
                        entry.Property("CreatedAt").CurrentValue = creationDate;
                    }
                }
                else
                {
                    entry.Property("CreatedAt").CurrentValue = creationDate;
                }
                if (updateAt != null)
                {
                    if (DateTime.Parse(updateAt.ToString()).Year > 1970)
                    {
                        entry.Property("UpdatedAt").CurrentValue = ((DateTime)updateAt).ToUniversalTime();
                    }
                    else
                    {
                        entry.Property("UpdatedAt").CurrentValue = creationDate;
                    }
                }
                else
                {
                    entry.Property("UpdatedAt").CurrentValue = creationDate;
                }
                entry.Property("CreatedAt").CurrentValue = creationDate;
                entry.Property("UpdatedAt").CurrentValue = creationDate;

                SaveChangeResult.AddMessage($"В ChangeTracker появились новые сущности: {entry.Entity.GetType()}");
            }

            var updateEntries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified);

            foreach (var en in updateEntries)
            {
                if (en.Entity is IAudiTable)
                {
                    //var userName = en.Property("UpdatedBy").CurrentValue == null
                    //    ? DefaultUserName : en.Property("UpdatedBy").CurrentValue;

                    en.Property("UpdatedAt").CurrentValue = DateTime.UtcNow.ToUniversalTime();
                    //en.Property("UpdatedBy").CurrentValue = userName;
                }

                SaveChangeResult.AddMessage($"ChangeTracker изменил сущности:: {en.Entity.GetType()}");
            }
        }
    }
}
