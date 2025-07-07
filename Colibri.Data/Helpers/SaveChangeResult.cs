using System;
using System.Collections.Generic;

namespace Colibri.Data.Helpers
{
    /// <summary>
    ///  ласс, представл¤ющий результат операции сохранени¤ изменений в базе данных.
    /// ’ранит информацию о сообщени¤х, исключени¤х и статусе выполнени¤ операции.
    /// </summary>
    public class SaveChangeResult
    {
        /// <summary>
        /// »нициализирует новый экземпл¤р класса <see cref="SaveChangeResult"/>.
        /// </summary>
        public SaveChangeResult()
        {
            Messages = [];
        }
        /// <summary>
        /// »нициализирует новый экземпл¤р класса <see cref="SaveChangeResult"/>,
        /// добавл¤¤ начальное сообщение.
        /// </summary>
        /// <param name="message">—ообщение, которое будет добавлено в список.</param>
        public SaveChangeResult(string message) : this()
        {
            AddMessage(message);
        }
        /// <summary>
        /// »сключение, возникшее во врем¤ операции сохранени¤.
        /// </summary>
        public Exception Exception { get; set; }
        /// <summary>
        /// ”казывает, успешна ли операци¤ сохранени¤.
        /// ¬озвращает true, если исключение равно null, иначе false.
        /// </summary>
        public bool IsOk => Exception == null;
        /// <summary>
        /// ƒобавл¤ет сообщение в список сообщений.
        /// </summary>
        /// <param name="message">—ообщение, которое нужно добавить.</param>
        public void AddMessage(string message)
        {
            Messages.Add(message);
        }
        /// <summary>
        /// —писок сообщений, св¤занных с результатом сохранени¤.
        /// </summary>
        private List<string> Messages { get; }
    }
}
