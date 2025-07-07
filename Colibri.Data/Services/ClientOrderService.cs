using Colibri.Data.Context;
using Colibri.Data.Entity;
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
                return await _context.Orders.FindAsync(id);
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
                return await _context.Orders.Where(x => x.UserId.ToGuid() == user.Id).ToListAsync();
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
                return await _context.Orders.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных списка всех карточек товаров", ex);
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
                return await _context.Products.FindAsync(id);
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
                return await _context.Products.Where(x => x.UserId == user.Id).ToListAsync();
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
                return await _context.Products.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных списка всех товаров", ex);
            }
        }
    }
}
