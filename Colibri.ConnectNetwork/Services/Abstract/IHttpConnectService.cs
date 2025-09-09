using System.Threading.Tasks;

namespace Colibri.ConnectNetwork.Services.Abstract
{
    /// <summary>
    /// Обеспечивает асинхронные методы для выполнения HTTP-запросов: GET, POST, PUT и DELETE.
    /// </summary>
    public interface IHttpConnectService
    {
        /// <summary>
        /// Выполняет асинхронный HTTP GET-запрос по указанному URL.
        /// </summary>
        /// <param name="url">Адрес ресурса, к которому осуществляется запрос.</param>
        /// <returns>Асинхронная задача, которая по завершении возвращает строку с ответом сервера.</returns>
        Task<string> GetAsync(string url);
        /// <summary>
        /// Выполняет асинхронный HTTP POST-запрос с передачей данных по указанному URL.
        /// </summary>
        /// <param name="url">Адрес ресурса, на который отправляется запрос.</param>
        /// <param name="data">Данные для отправки в теле POST-запроса.</param>
        /// <returns>Асинхронная задача, которая по завершении возвращает строку с ответом сервера.</returns>
        Task<string> PostAsync(string url, string data);
        /// <summary>
        /// Выполняет асинхронный HTTP PUT-запрос для обновления ресурса по указанному URL.
        /// </summary>
        /// <param name="url">Адрес ресурса, который необходимо обновить.</param>
        /// <param name="data">Данные для отправки в теле PUT-запроса.</param>
        /// <returns>Асинхронная задача, которая по завершении возвращает строку с ответом сервера.</returns>
        Task<string> PutAsync(string url, string data);
        /// <summary>
        /// Выполняет асинхронный HTTP DELETE-запрос по указанному URL для удаления ресурса.
        /// </summary>
        /// <param name="url">Адрес ресурса, который необходимо удалить.</param>
        /// <returns>Асинхронная задача, которая по завершении возвращает строку с ответом сервера.</returns>
        Task<string> DeleteAsync(string url);
    }
}
