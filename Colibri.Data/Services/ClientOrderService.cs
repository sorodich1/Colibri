using Colibri.Data.Context;
using Colibri.Data.Entity;
using Colibri.Data.Helpers;
using Colibri.Data.Services.Abstracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colibri.Data.Services
{
    /// <summary>
    /// Сервис для управления заказами и продуктами клиента.
    /// Реализует интерфейс <see cref="IClientOrderService"/>.
    /// <param name="context">Контекст базы данных.</param>
    /// </summary>
    public class ClientOrderService(AppDbContext context) : IClientOrderService
    {
        private readonly AppDbContext _context = context;
        /// <summary>
        /// Создает новый заказ в базе данных.
        /// </summary>
        /// <param name="order">Объект заказа, который необходимо создать.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если заказ успешно создан; в противном случае <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<bool> CreadOrdersAsync(Order order)
        {
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка сохранения в базе данных карточки товара", ex);
            }
        }
        /// <summary>
        /// Создает новый продукт в базе данных.
        /// </summary>
        /// <param name="product">Объект продукта, который необходимо создать.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если продукт успешно создан; в противном случае <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<bool> CreadProductAsync(Product product)
        {
            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка сохранения в базе данных товара", ex);
            }
        }
        /// <summary>
        /// Получает заказ по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор заказа.</param>
        /// <returns>Асинхронная задача, возвращающая объект заказа с указанным идентификатором или <c>null</c>, если заказ не найден.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<Order> GetOrderByIdAsync(int id)
        {
            try
            {
                return await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных карточки товара по идентификатору", ex);
            }
        }
        /// <summary>
        /// Выборка карточек товара по имени пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>Результат запроса в виде списка карточек товара выбранных по имени пользователя</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<List<Order>> GetOrderByUserAsync(User user)
        {
            try
            {
                return await _context.Orders.Where(x => x.UserId.ToGuid() == user.Id && !x.IsDeleted).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных карточки товара по идентификатору", ex);
            }
        }
        /// <summary>
        /// Получает список всех заказов.
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список объектов заказов.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<List<Order>> GetOrdersAsync()
        {
            try
            {
                return await _context.Orders.Where(x => !x.IsDeleted).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных списка всех карточек товаров", ex);
            }
        }

        /// <summary>
        /// Получает последние 5 заказов для конкретного клиента.
        /// </summary>
        /// <param name="user">Пользователь (клиент).</param>
        /// <returns>Асинхронная задача, возвращающая список последних 5 заказов клиента.</returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибке выборки из базы данных</exception>
        public async Task<List<Order>> GetLastFiveOrdersByUserAsync(User user)
        {
            try
            {
                return await _context.Orders
                    .Where(x => x.UserId.ToGuid() == user.Id && x.IsDeleted)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки последних 5 заказов для пользователя", ex);
            }
        }
        /// <summary>
        /// Получает продукт по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор продукта.</param>
        /// <returns>Асинхронная задача, возвращающая объект продукта с указанным идентификатором или <c>null</c>, если продукт не найден.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<Product> GetProductByIdAsync(int id)
        {
            try
            {
                return await _context.Products.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных товара по идентификатору", ex);
            }
        }
        /// <summary>
        /// Выборка всех товаров назначенных конкретному пользователю
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>Список товаров по идентификатору пользователя</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<List<Product>> GetProductByUserAsync(User user)
        {
            try
            {
                return await _context.Products.Where(x => x.UserId == user.Id && !x.IsDeleted).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных товара по имени пользователя", ex);
            }
        }
        
        /// <summary>
        /// Получает список всех продуктов.
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список объектов продуктов.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                return await _context.Products.Where(x => !x.IsDeleted).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных списка всех товаров", ex);
            }
        }

        /// <summary>
        /// Удаляет заказ по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор заказа.</param>
        /// <returns>Асинхронная задача, возвращающая true, если удаление успешно.</returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибке удаления</exception>
        public async Task<bool> DeleteOrderAsync(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                
                if (order == null)
                {
                    return false;
                }

                order.IsDeleted = true; // Мягкое удаление
                order.UpdatedAt = DateTime.UtcNow;
                
                // Или для полного удаления:
                // _context.Orders.Remove(order);
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка удаления заказа из базы данных", ex);
            }
        }

        /// <summary>
        /// Удаляет продукт по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор продукта.</param>
        /// <returns>Асинхронная задача, возвращающая true, если удаление успешно.</returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибке удаления</exception>
        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                
                if (product == null)
                {
                    return false;
                }

                // Мягкое удаление, если есть поле IsDeleted
                if (_context.Model.FindEntityType(typeof(Product)).FindProperty("IsDeleted") != null)
                {
                    var isDeletedProperty = typeof(Product).GetProperty("IsDeleted");
                    if (isDeletedProperty != null && isDeletedProperty.CanWrite)
                    {
                        isDeletedProperty.SetValue(product, true);
                    }
                    
                    var updatedAtProperty = typeof(Product).GetProperty("UpdatedAt");
                    if (updatedAtProperty != null && updatedAtProperty.CanWrite)
                    {
                        updatedAtProperty.SetValue(product, DateTime.UtcNow);
                    }
                }
                else
                {
                    // Полное удаление
                    _context.Products.Remove(product);
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка удаления продукта из базы данных", ex);
            }
        }

        public async Task<bool> UpdateOrdersAsync(Order order)
        {
            try
            {
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка обнавлении в базе данных карточки товара", ex);
            }
        }
    }
}
