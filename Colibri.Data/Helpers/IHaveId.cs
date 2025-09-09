namespace Colibri.Data.Helpers
{
    /// <summary>
    /// интерфейс идентификатора сущностей
    /// </summary>
    public interface IHaveId
    {
        /// <summary>
        /// Идентификатор сущностей
        /// </summary>
        int Id { get; set; }
    }
}
