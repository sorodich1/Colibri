using Colibri.ConnectNetwork.Services.Abstract;
using System;
using System.Net.Sockets;
using System.Text;

namespace Colibri.ConnectNetwork.Services
{
    /// <summary>
    /// Реализация сервиса для TCP-подключений, предоставляющая методы для подключения, отправки, получения и отключения.
    /// </summary>
    public class TcpConnectService : ITcpConnectService
    {
        /// <summary>
        /// Объект TcpClient для установления TCP-соединения.
        /// </summary>
        private TcpClient _client;
        /// <summary>
        /// Объект NetworkStream для обмена данными по TCP.
        /// </summary>
        private NetworkStream _stream;

        /// <summary>
        /// Объект TcpClient для установления TCP-соединения.
        /// </summary>
        /// <param name="host">Адрес сервера для подключения.</param>
        /// <param name="port">Порт для подключения.</param>
        /// <exception cref="InvalidOperationException">Возникает при ошибке подключения.</exception>
        public void Connect(string host, int port)
        {
            try
            {
                _client = new TcpClient(host, port);
                _stream = _client.GetStream();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка подключения по протоколу TCP", ex);
            }
        }

        /// <summary>
        /// Отключает текущие TCP-соединение.
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает при ошибке отключения.</exception>
        public void Disconnect()
        {
            try
            {
                _stream.Close();
                _client.Close();
                _stream = null;
                _client = null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка одключение по протоколу TCP", ex);
            }
        }

        /// <summary>
        /// Получает сообщение, отправленное по TCP-соединению.
        /// </summary>
        /// <returns>Строка с полученными данными.</returns>
        /// <exception cref="InvalidOperationException">Возникает, если клиент не подключен или при ошибке чтения.</exception>
        public string Receive()
        {
            try
            {
                if (_stream == null)
                {
                    throw new InvalidOperationException("Клиент не подключен");
                }
                byte[] buffer = new byte[1024];
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка подключения клиента по протоколу TCP", ex);
            }
        }

        /// <summary>
        /// Отправляет сообщение по TCP-соединению.
        /// </summary>
        /// <param name="message">Сообщение для отправки.</param>
        /// <exception cref="InvalidOperationException">Возникает, если клиент не подключен или при ошибке отправки.</exception>
        public void Send(string message)
        {
            try
            {
                if (_stream == null)
                {
                    throw new InvalidOperationException("Клиент не подключен");
                }
                byte[] data = Encoding.UTF8.GetBytes(message);
                _stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка передачи данных по протоколу TCP", ex);
            }
        }
    }
}
