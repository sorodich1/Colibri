using Colibri.Data.Helpers;
using System;

namespace Colibri.Data.Entity
{
    /// <summary>
    /// Сущность товара
    /// </summary>
    public class Product : AudiTable
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Имя товара
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Стоимость товара
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Колличество на складе
        /// </summary>
        public int QuantityInStock { get; set; }
        /// <summary>
        /// Категория товара
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// Картинка товара
        /// </summary>
        public byte[] Image { get; set; }
    }
}
