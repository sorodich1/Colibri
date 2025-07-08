using Colibri.ConnectNetwork.Services.Abstract;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Colibri.ConnectNetwork.Services
{
    public class HttpConnectService : IHttpConnectService
    {
        private readonly HttpClient _client = new ();

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
                throw new InvalidOperationException("Ошибка передачи POST запроса по протоколу HTTP", ex);
            }
        }

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
