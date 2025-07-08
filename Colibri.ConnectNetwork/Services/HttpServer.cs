using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Colibri.ConnectNetwork.Services
{
    public class HttpServer
    {
        private readonly HttpListener _listener = new ();

        public void Start(string prefic)
        {
            _listener.Prefixes.Add(prefic);
            _listener.Start();

            Console.WriteLine("Сервер запущен...");

            Task.Run(() => AcceptClient());
        }

        private async Task AcceptClient()
        {
            while (true)
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(context));
            }
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            Console.WriteLine("Подключение клиента...");

            string responseString = string.Empty;

            switch (context.Request.HttpMethod)
            {
                case "GET":
                    responseString = "Поступил GET запрос";
                    break;
                case "POST":
                    using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        responseString = await reader.ReadToEndAsync();
                        Console.WriteLine("POST запрос: " + responseString);
                    }
                    break;
                case "PUT":
                    using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        responseString = await reader.ReadToEndAsync();
                        Console.WriteLine("PUT запрос: " + responseString);
                    }
                    break;
                case "DELETE":
                    responseString = "Ресурс удалён";
                    Console.WriteLine("Поступил DELETE запрос");
                    break;
                default:
                    responseString = "Метод не найден";
                    context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    break;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Close();
            Console.WriteLine("Ответ отправлен");
        }

        public void Stop()
        {
            _listener.Stop();
        }
    }
}
