using System.Threading.Tasks;
using Colibri.WebApi.Models;

namespace Colibri.WebApi.Services.Abstract;
/// <summary>
/// 
/// </summary>
public interface IHomePositionService
{
    /// <summary>
    /// Установить/обновить домашнюю позицию
    /// </summary>
    Task<bool> SetHomePosition(GeoPoint position);
    
    /// <summary>
    /// Получить текущую домашнюю позицию
    /// </summary>
    Task<GeoPoint> GetHomePosition();
    
    /// <summary>
    /// Проверить, установлена ли домашняя позиция
    /// </summary>
    Task<bool> IsHomePositionSet();
    
    /// <summary>
    /// Сбросить домашнюю позицию (для тестов)
    /// </summary>
    Task<bool> ResetHomePosition();
}
