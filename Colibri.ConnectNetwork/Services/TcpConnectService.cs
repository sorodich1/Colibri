using Colibri.ConnectNetwork.Services.Abstract;
using System;
using System.Net.Sockets;
using System.Text;

namespace Colibri.ConnectNetwork.Services
{
    public class TcpConnectService : ITcpConnectService
    {
        private TcpClient _client;
        private NetworkStream _stream;

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
