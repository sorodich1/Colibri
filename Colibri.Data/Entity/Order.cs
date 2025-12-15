using Colibri.Data.Helpers;

namespace Colibri.Data.Entity
{
    /// <summary>
    /// Сущность карточки товара
    /// </summary>
    public class Order : AudiTable
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Идентификатор продукта
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Количество
        /// </summary>
        public int Quentity { get; set; }
        /// <summary>
        /// Широта места доставки
        /// </summary>
        public decimal DeliveryLatitude { get; set; }
        /// <summary>
        /// Долгота места доставки
        /// </summary>
        public decimal DeliveryLongitude { get; set; }
        /// <summary>
        /// 
        /// Статус доставки
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedBy { get; set; }
    }
}
