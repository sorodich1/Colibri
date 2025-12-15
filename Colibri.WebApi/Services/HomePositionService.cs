using System;
using System.IO;
using System.Threading.Tasks;
using Colibri.Data.Services.Abstracts;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Colibri.WebApi.Services;
/// <summary>
/// 
/// </summary>
public class HomePositionService : IHomePositionService
{
    private GeoPoint _homePosition = null;
    private bool _homePositionSet = false;
    private const string HOME_POSITION_FILE = "home_position.json";
    private readonly ILoggerService _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public HomePositionService(ILoggerService logger)
    {
        _logger = logger;
        
        LoadHomePositionFromStorage().Wait();
    }

    public async Task<bool> SetHomePosition(GeoPoint position)
    {
        try
        {
            _homePosition = position;
            _homePositionSet = true;
            
            _logger.LogMessage(null, $"Домашняя позиция УСТАНОВЛЕНА: Lat={position.Latitude:F6}, Lon={position.Longitude:F6}", LogLevel.Debug);
            
            // Сохраняем в файл для persistence
            await SaveHomePositionToStorage(position);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogMessage(null, $"Ошибка установки домашней позиции: {ex.Message}", LogLevel.Error);
            return false;
        }
    }

    public async Task<GeoPoint> GetHomePosition()
    {
        if (_homePositionSet)
        {
            return _homePosition;
        }
        
        // Пытаемся загрузить из хранилища
        var savedPosition = await LoadHomePositionFromStorage();
        if (savedPosition != null)
        {
            _homePosition = savedPosition;
            _homePositionSet = true;
            _logger.LogMessage(null, "Домашняя позиция загружена из хранилища", LogLevel.Debug);
            return savedPosition;
        }
        
        return null;
    }

    public Task<bool> IsHomePositionSet()
    {
        return Task.FromResult(_homePositionSet || File.Exists(HOME_POSITION_FILE));
    }

    public async Task<bool> ResetHomePosition()
    {
        try
        {
            _homePosition = null;
            _homePositionSet = false;
            
            // Удаляем файл если существует
            if (File.Exists(HOME_POSITION_FILE))
            {
                File.Delete(HOME_POSITION_FILE);
            }
            
            _logger.LogMessage(null, $"Домашняя позиция сброшена", LogLevel.Debug);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogMessage(null, $"Ошибка сброса домашней позиции: {ex.Message}", LogLevel.Error);
            return false;
        }
    }


    #region Private Method

    private async Task<GeoPoint> LoadHomePositionFromStorage()
    {
        try
        {
            if (File.Exists(HOME_POSITION_FILE))
            {
                var json = await File.ReadAllTextAsync(HOME_POSITION_FILE);
                var homeData = JsonConvert.DeserializeObject<dynamic>(json);
                
                var position = new GeoPoint
                {
                    Latitude = (double)homeData.Latitude,
                    Longitude = (double)homeData.Longitude
                };
                
                _logger.LogMessage(null, $"Домашняя позиция загружена из файла: {HOME_POSITION_FILE}", LogLevel.Debug);
                return position;
            }
        }
        catch (Exception ex)
        {
            _logger.LogMessage(null, $"Не удалось загрузить домашнюю позицию из файла: {ex.Message}", LogLevel.Error);
        }
        
        return null;
    }

    private async Task SaveHomePositionToStorage(GeoPoint position)
    {
        try
        {
            var homeData = new
            {
                Latitude = position.Latitude,
                Longitude = position.Longitude,
                SetTime = DateTime.UtcNow
            };
            
            var json = JsonConvert.SerializeObject(homeData, Formatting.Indented);
            await File.WriteAllTextAsync(HOME_POSITION_FILE, json);
            

            _logger.LogMessage(null, $"Домашняя позиция сохранена в файл: {HOME_POSITION_FILE}", LogLevel.Debug);
        }
        catch (Exception ex)
        {
             _logger.LogMessage(null, $"Не удалось сохранить домашнюю позицию в файл: {ex.Message}", LogLevel.Error);
        }
    }

    #endregion
}
