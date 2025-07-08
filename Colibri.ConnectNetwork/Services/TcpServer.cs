using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Colibri.ConnectNetwork.Services
{
    public class TcpServer
    {
        private TcpListener _listener;

        public void Start(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            Console.WriteLine("Server is listener... ");

            Task.Run(() => AcceptClients());
        }

        private async Task AcceptClients()
        {
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                await Task.Run(() => HandleClient(client));
            }
        }

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

        public void Stop()
        {
            _listener.Stop();
        }
    }
}
