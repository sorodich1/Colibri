using System;
using System.Text.Json;
using System.Threading.Tasks;
using Colibri.ConnectNetwork.Services.Abstract;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Colibri.WebApi.Services;

public class DroneConnectionService : IDroneConnectionService
{
    private readonly IHttpConnectService _httpConnect;
    private readonly ILogger<DroneConnectionService> _logger;

    // Фиксированный URL дрона из контроллера
    //private const string DRONE_BASE_URL = "http://85.141.101.21:8080";

    private const string DRONE_BASE_URL = "http://192.168.1.159:8080";

    public DroneConnectionService(IHttpConnectService httpConnect, ILogger<DroneConnectionService> logger)
    {
        _httpConnect = httpConnect;
        _logger = logger;
    }

    public async Task<string> GetActiveDroneUrl()
    {
        // Всегда возвращаем фиксированный URL
        return DRONE_BASE_URL;
    }

    public async Task<DroneConnectionResult> SendCommandToDrone(string endpoint, object command)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            
            // Формируем полный URL
            var url = $"{DRONE_BASE_URL}/{endpoint.TrimStart('/')}";
            
            string response;
            
            // Для GET запросов (status)
            if (endpoint == "status")
            {
                response = await _httpConnect.GetAsync(url);
                // ПРОВЕРЯЕМ что ответ не пустой и валидный
                if (string.IsNullOrEmpty(response))
                {
                    return new DroneConnectionResult
                    {
                        Success = false,
                        ErrorMessage = "Пустой ответ от дрона"
                    };
                }
            }
            // Для POST запросов
            else
            {
               // var jsonCommand = JsonConvert.SerializeObject(command);
                
                string jsonCommand = System.Text.Json.JsonSerializer.Serialize(command);
                response = await _httpConnect.PostAsync(url, jsonCommand);
                
                // ПРОВЕРЯЕМ ответ дрона
                if (!string.IsNullOrEmpty(response))
                {
                    // Парсим ответ чтобы проверить статус
                    var responseData = JsonConvert.DeserializeObject<dynamic>(response);
                    if (responseData.status == "mission_failed" || responseData.error != null)
                    {
                        return new DroneConnectionResult
                        {
                            Success = false,
                            ErrorMessage = $"Дрон вернул ошибку: {responseData.error?.ToString() ?? responseData.status?.ToString()}"
                        };
                    }
                }
            }
            
            _logger.LogInformation($"Команда успешно отправлена на дрон: {url}");

            return new DroneConnectionResult
            {
                Success = true,
                DroneUrl = DRONE_BASE_URL,
                ResponseTime = DateTime.UtcNow - startTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Не удалось отправить команду на дрон: {ex.Message}");
            
            return new DroneConnectionResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}