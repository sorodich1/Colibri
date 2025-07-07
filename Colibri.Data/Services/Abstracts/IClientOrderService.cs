using Colibri.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Colibri.Data.Services.Abstracts
{
    /// <summary>
    /// Интерфейс для управления заказами и продуктами клиента.
    /// Предоставляет методы для создания и получения заказов и продуктов.
    /// </summary>
    public interface IClientOrderService
    {
        /// <summary>
        ///  Создает новый заказ.
        /// </summary>
        /// <param name="order">Объект заказа, который необходимо создать.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если заказ успешно создан; в противном случае <c>false</c>.</returns>
        Task<bool> CreadOrdersAsync(Order order);
        /// <summary>
        /// Создает новый продукт.
        /// </summary>
        /// <param name="product">Объект продукта, который необходимо создать.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если продукт успешно создан; в противном случае <c>false</c>.</returns>
        Task<bool> CreadProductAsync(Product product);
        /// <summary>
        /// Получает список всех заказов.
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список объектов заказов.</returns>
        Task<List<Order>> GetOrdersAsync();
        /// <summary>
        /// Получает список всех продуктов.
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список объектов продуктов.</returns>
        Task<List<Product>> GetProductsAsync();
        /// <summary>
        /// Получает заказ по уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор заказа.</param>
        /// <returns>Асинхронная задача, возвращающая объект заказа с указанным идентификатором или <c>null</c>, если заказ не найден.</returns>
        Task<Order> GetOrderByIdAsync(int id);
        /// <summary>
        /// Получает продукт по уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор продукта.</param>
        /// <returns>Асинхронная задача, возвращающая объект продукта с указанным идентификатором или <c>null</c>, если продукт не найден.</returns>
        Task<Product> GetProductByIdAsync(int id);
        /// <summary>
        /// Список продуктов конкретного пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>Список товаров по имени пользователя</returns>
        Task<List<Product>> GetProductByUserAsync(User user);
        /// <summary>
        /// Список карточек заказа конкретного пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>Список товаров по имени пользователя</returns>
        Task<List<Order>> GetOrderByUserAsync(User user);
    }
}
