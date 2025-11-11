using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Colibri.ConnectNetwork.Services.Abstract;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Colibri.WebApi.Services;

public class DroneConnectionService : IDroneConnectionService
{
    private readonly IHttpConnectService _httpConnect;
    private readonly ILogger<DroneConnectionService> _logger;
    private readonly List<DroneConfig> _drones;
    private string _currentDroneUrl;

    public DroneConnectionService(IHttpConnectService httpConnect, IConfiguration configuration, ILogger<DroneConnectionService> logger)
    {
        _httpConnect = httpConnect;
        _logger = logger;
        _drones = LoadDronesFromConfig(configuration);
        _currentDroneUrl = _drones.FirstOrDefault()?.BaseUrl;
    }



    public async Task<string> GetActiveDroneUrl()
    {
        // Проверяем, что текущий дрон еще доступен
        if (!string.IsNullOrEmpty(_currentDroneUrl) && await TestDroneConnection(_currentDroneUrl))
        {
            return _currentDroneUrl;
        }
        // Ищем следующий доступный Url
        foreach (var drone in _drones.Where(d => d.IsActive).OrderBy(d => d.Priority))
        {
            if (await TestDroneConnection(drone.BaseUrl))
            {
                _currentDroneUrl = drone.BaseUrl;
                return _currentDroneUrl;
            }
        }
        return null;
    }

    public List<DroneConfig> GetAvailableDrones()
    {
        throw new NotImplementedException();
    }

    public async Task<DroneConnectionResult> SendCommandToDrone(string endpoint, object command)
    {
        var exceptions = new List<Exception>();
        var activeDrones = _drones.Where(d => d.IsActive).OrderBy(d => d.Priority).ToList();

        foreach (var drone in activeDrones)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                
                // Исправляем endpoint-ы под реальные пути дрона
                string actualEndpoint = endpoint switch
                {
                    "api/health" => "status",           // Дрон использует /status
                    "api/mission/upload" => "execute-mission", // Дрон использует /execute-mission
                    _ => endpoint
                };
                
                var url = $"{drone.BaseUrl}/{actualEndpoint.TrimStart('/')}";
                
                // Для GET запросов (status)
                if (actualEndpoint == "status")
                {
                    var response = await _httpConnect.GetAsync(url);
                }
                // Для POST запросов (execute-mission)
                else
                {
                    var jsonCommand = JsonConvert.SerializeObject(command);
                    var response = await _httpConnect.PostAsync(url, jsonCommand);
                }
                
                _currentDroneUrl = drone.BaseUrl;
                _logger.LogInformation($"Команда успешно отправлена на дрон: {drone.Name} ({url})");

                return new DroneConnectionResult
                {
                    Success = true,
                    DroneUrl = drone.BaseUrl,
                    ResponseTime = DateTime.UtcNow - startTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Не удалось отправить команду на дрон {drone.Name}: {ex.Message}");
                exceptions.Add(ex);
                continue;
            }
        }

        return new DroneConnectionResult
        {
            Success = false,
            ErrorMessage = string.Join("; ", exceptions.Select(e => e.Message))
        };
    }

    public async Task SwitchToNextDrone()
    {
        var currentIndex = _drones.FindIndex(d => d.BaseUrl == _currentDroneUrl);
        var nextIndex = (currentIndex + 1) % _drones.Count;

        for (int i = 0; i < _drones.Count; i++)
        {
            var drone = _drones[nextIndex];
            if (drone.IsActive && await TestDroneConnection(drone.BaseUrl))
            {
                _currentDroneUrl = drone.BaseUrl;
                _logger.LogInformation($"Переключились на url: {drone.Name}");
                return;
            }
            nextIndex = (nextIndex + 1) % _drones.Count;
        }

        throw new InvalidOperationException("Нет доступных url для подключения");
    }

    public async Task<bool> TestDroneConnection(string droneUrl)
    {
        try
        {
            // Отправляем тестовый запрос для проверки связи
            var response = await _httpConnect.GetAsync($"{droneUrl}/api/health");
            return true;
        }
        catch
        {
            return false;
        }
    }

    private List<DroneConfig> LoadDronesFromConfig(IConfiguration configuration)
    {
        var drones = new List<DroneConfig>
        {
            new() { Name = "Основной url", BaseUrl = "http://78.25.108.95:8080", Priority = 1, IsActive = true },
            new() { Name = "Резервный url 1", BaseUrl = "http://85.141.101.21:8080", Priority = 2, IsActive = true },
            new() { Name = "Резервный url 2", BaseUrl = "http://192.168.1.100:8080", Priority = 3, IsActive = true }
        };

        return [.. drones.OrderBy(d => d.Priority)];
    }
}   