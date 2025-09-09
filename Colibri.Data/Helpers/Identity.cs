namespace Colibri.Data.Helpers
{
    /// <summary>
    /// Вспомогательный базовый класс сущностей
    /// </summary>
    public class Identity : IHaveId
    {
        /// <summary>
        /// Идентификатор сущности
        /// </summary>
        public int Id { get; set; }
    }
}
