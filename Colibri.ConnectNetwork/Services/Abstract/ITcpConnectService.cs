namespace Colibri.ConnectNetwork.Services.Abstract
{
    /// <summary>
    /// Обеспечивает интерфейс для TCP-соединения с возможностью подключения, отправки и получения данных, а также отключения.
    /// </summary>
    public interface ITcpConnectService
    {
        /// <summary>
        /// Устанавливает TCP-соединение с указанным хостом и портом.
        /// </summary>
        /// <param name="host">Адрес сервера для подключения.</param>
        /// <param name="port">Порт сервера для подключения.</param>
        void Connect(string host, int port);
        /// <summary>
        /// Отправляет сообщение через установленное TCP-соединение.
        /// </summary>
        /// <param name="message">Сообщение для отправки.</param>
        void Send(string message);
        /// <summary>
        /// Получает сообщение, пришедшее через установленное TCP-соединение.
        /// </summary>
        /// <returns></returns>
        string Receive();
        /// <summary>
        /// Прерывает текущее TCP-соединение и освобождает ресурсы.
        /// </summary>
        void Disconnect();
    }
}
