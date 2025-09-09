using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Colibri.ConnectNetwork.Services
{
    /// <summary>
    /// Реализация TCP-сервера, который слушает входящие подключения и обрабатывает сообщения клиентов.
    /// </summary>
    public class TcpServer
    {
        private TcpListener _listener;
        /// <summary>
        /// Запускает сервер на указанном порту и начинает слушать входящие соединения.
        /// </summary>
        /// <param name="port">Порт, на котором будет запущен сервер.</param>
        public void Start(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            Console.WriteLine("Server is listener... ");

            Task.Run(() => AcceptClients());
        }
        /// <summary>
        /// Асинхронно принимает входящие подключения и создает для каждого клиента отдельную задачу для обработки.
        /// </summary>
        private async Task AcceptClients()
        {
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                await Task.Run(() => HandleClient(client));
            }
        }
        /// <summary>
        /// Обрабатывает подключенного клиента: читает сообщения и отправляет обратно.
        /// </summary>
        /// <param name="client">Объект клиента TCP, с которым происходит взаимодействие.</param>
        private async Task HandleClient(TcpClient client)
        {
            Console.WriteLine("Клиент подключен.");

            var stream = client.GetStream();

            while (client.Connected)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine("Ответ от сервера: " + message);

                await stream.WriteAsync(buffer.AsMemory(0, bytesRead));
            }
            client.Close();
            Console.WriteLine("Соединение закрыто.");
        }
        /// <summary>
        /// Останавливает сервер, прекращая слушать входящие подключения.
        /// </summary>
        public void Stop()
        {
            _listener.Stop();
        }
    }
}
