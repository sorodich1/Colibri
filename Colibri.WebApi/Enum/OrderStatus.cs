using System.ComponentModel.DataAnnotations;

namespace Colibri.WebApi.Enum
{
    /// <summary>
    /// Перечисления статусов заказа
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// аказ создан
        /// </summary>
        [Display(Name = "Заказ создан")]
        Created,
        /// <summary>
        /// заказ вылетел
        /// </summary>
        [Display(Name = "Заказ вылетел")]
        Dispatched,
        /// <summary>
        /// Заказ в пути
        /// </summary>
        [Display(Name = "Заказ в пути")]
        InTransit,
        /// <summary>
        /// Заказ прибудет через 3 минуты
        /// </summary>
        [Display(Name = "Заказ прибудет через 3 минуты")]
        ArrivingSoon,
        /// <summary>
        /// Заказ прибыл
        /// </summary>
        [Display(Name = "Заказ прибыл")]
        Arrived,
        /// <summary>
        /// Заказ завершён
        /// </summary>
        [Display(Name = "Заказ завершён")]
        Completed,
        /// <summary>
        /// Заказ отменён
        /// </summary>
        [Display(Name = "Заказ отменён")]
        Canceled
    }
}
