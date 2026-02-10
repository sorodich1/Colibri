using System.ComponentModel.DataAnnotations;

namespace Colibri.WebApi.Enum
{
    /// <summary>
    /// Перечисления статусов заказа
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// заказ создан
        /// </summary>
        [Display(Name = "Заказ создан")]
        PREPARING,
        /// <summary>
        /// заказ Собирается
        /// </summary>
        [Display(Name = "Заказ собирается")]
        IS_GOING_TO,
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
