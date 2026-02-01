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
        PREPARING,
        /// <summary>
        /// заказ вылетел
        /// </summary>
        [Display(Name = "Заказ вылетел")]
        IN_TRANSIT,
        /// <summary>
        /// Заказ прибыл
        /// </summary>
        [Display(Name = "Заказ прибыл")]
        DELIVERED
    }
}
