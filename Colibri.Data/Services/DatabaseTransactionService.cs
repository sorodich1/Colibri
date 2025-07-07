using Colibri.Data.Context;
using Colibri.Data.Services.Abstracts;
using Microsoft.EntityFrameworkCore.Storage;

namespace Colibri.Data.Services
{
    /// <summary>
    /// Сервис для управления транзакциями базы данных.
    /// Реализует интерфейс <see cref="IDatabaseTransactionService"/>.
    /// </summary>
    public class DatabaseTransactionService(AppDbContext context) : IDatabaseTransactionService
    {
        private readonly AppDbContext _context = context;
        private IDbContextTransaction _transaction;

        /// <summary>
        /// Начинает новую транзакцию. 
        /// Должен вызываться перед выполнением отдельных операций, которые должны быть атомарными.
        /// </summary>
        public void BeginTransaction()
        {
            _transaction = _context.Database.BeginTransaction();
        }
        /// <summary>
        /// Подтверждает текущую транзакцию, сохраняя все изменения в базе данных.
        /// После вызова этого метода все изменения, сделанные в рамках транзакции, становятся постоянными.
        /// </summary>
        public void CommitTransaction()
        {
            _transaction?.Commit();
        }
        /// <summary>
        /// Откатывает текущую транзакцию, отменяя все изменения, сделанные до вызова этого метода.
        /// </summary>
        public void RollbackTransaction()
        {
            _transaction?.Rollback();
        }
    }
}
