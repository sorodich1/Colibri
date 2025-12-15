using Colibri.ConnectNetwork.Services.Abstract;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Colibri.ConnectNetwork.Services
{
    /// <summary>
    /// Реализует сервис для выполнения HTTP-запросов (GET, POST, PUT, DELETE).
    /// </summary>
    public class HttpConnectService : IHttpConnectService
    {
        /// <summary>
        /// Экземпляр HttpClient, используемый для отправки HTTP-запросов.
        /// </summary>
        private readonly HttpClient _client = new();

        /// <summary>
        /// Выполняет асинхронный HTTP DELETE-запрос по указанному URL.
        /// </summary>
        /// <param name="url">URL-адрес для отправки DELETE-запроса.</param>
        /// <returns>Ответ сервера в виде строки.</returns>
        /// <exception cref="InvalidOperationException">Выбрасывается при ошибке выполнения запроса.</exception>
        public async Task<string> DeleteAsync(string url)
        {
            try
            {
                var response = await _client.DeleteAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка передачи DELETE запроса по протоколу HTTP", ex);
            }
        }
        /// <summary>
        /// Выполняет асинхронный HTTP GET-запрос по указанному URL.
        /// </summary>
        /// <param name="url">URL-адрес для отправки GET-запроса.</param>
        /// <returns>Ответ сервера в виде строки.</returns>
        /// <exception cref="InvalidOperationException">Выбрасывается при ошибке выполнения запроса.</exception>
        public async Task<string> GetAsync(string url)
        {
            try
            {
                var response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка передачи GET запроса по протоколу HTTP", ex);
            }
        }

        /// <summary>
        /// Выполняет асинхронный HTTP POST-запрос с данными по указанному URL.
        /// </summary>
        /// <param name="url">URL-адрес для отправки POST-запроса.</param>
        /// <param name="data">Данные для передачи в теле запроса (обычно в формате JSON).</param>
        /// <returns>Ответ сервера в виде строки.</returns>
        /// <exception cref="InvalidOperationException">Выбрасывается при ошибке выполнения запроса.</exception>
        public async Task<string> PostAsync(string url, string data)
        {
            try
            {
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                Console.WriteLine(url, data);
                var response = await _client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка передачи POST запроса по протоколу HTTP - {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Выполняет асинхронный HTTP PUT-запрос с данными по указанному URL.
        /// </summary>
        /// <param name="url">URL-адрес для отправки PUT-запроса.</param>
        /// <param name="data">Данные для передачи в теле запроса (обычно в формате JSON).</param>
        /// <returns>Ответ сервера в виде строки.</returns>
        /// <exception cref="InvalidOperationException">Выбрасывается при ошибке выполнения запроса.</exception>
        public async Task<string> PutAsync(string url, string data)
        {
            try
            {
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                var response = await _client.PutAsync(url, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка передачи PUT запроса по протоколу HTTP", ex);
            }
        }
    }
}
