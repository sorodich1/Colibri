using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Colibri.ConnectNetwork.Services
{
    /// <summary>
    /// Реализует простой HTTP-сервер на базе HttpListener для обработки входящих запросов.
    /// </summary>
    public class HttpServer
    {
        /// <summary>
        /// Экземпляр HttpListener, который слушает входящие HTTP-запросы.
        /// </summary>
        private readonly HttpListener _listener = new();

        /// <summary>
        /// Запускает сервер и начинает прослушивание запросов по указанному префиксу.
        /// </summary>
        /// <param name="prefic">Префикс URL, по которому сервер будет слушать запросы (например, "http://localhost:8080/").</param>
        public void Start(string prefic)
        {
            _listener.Prefixes.Add(prefic);
            _listener.Start();

            Console.WriteLine("Сервер запущен...");

            Task.Run(() => AcceptClient());
        }

        /// <summary>
        /// Асинхронно принимает входящие соединения и инициирует обработку каждого клиента.
        /// </summary>
        private async Task AcceptClient()
        {
            while (true)
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(context));
            }
        }

        /// <summary>
        /// Обрабатывает входящий HTTP-запрос и формирует ответ.
        /// </summary>
        /// <param name="context">Контекст запроса, содержащий сведения о запросе и ответе.</param>
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

        /// <summary>
        /// Останавливает сервер и прекращает прослушивание входящих запросов.
        /// </summary>
        public void Stop()
        {
            _listener.Stop();
        }
    }
}
