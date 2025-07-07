using System;

namespace Colibri.Data.Entity
{
    /// <summary>
    /// логирование
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Идентификатор лога
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Уровень лога
        /// </summary>
        public string Level { get; set; }
        /// <summary>
        /// Пользователь
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// сообщение лога
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Место возникновения лога
        /// </summary>
        public string Logger { get; set; }
        /// <summary>
        /// Время и дата возникновения лога
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
