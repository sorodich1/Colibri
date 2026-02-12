namespace Colibri.WebApi.Models
{
    /// <summary>
    /// Модель карточки заказа
    /// </summary>
    public class OrderModel
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Идентификатор товара
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Колличество товаров
        /// </summary>
        public int Quentity { get; set; }
        /// <summary>
        /// Широта точки доставки
        /// </summary>
        public double DeliveryLatitude { get; set; }
        /// <summary>
        /// Долгота точки доставки
        /// </summary>
        public double DeliveryLongitude { get; set; }
    }
}
