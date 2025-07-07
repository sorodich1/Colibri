using Colibri.Data.Context;
using Colibri.Data.Entity;
using Colibri.Data.Services.Abstracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Colibri.Data.Services
{
    /// <summary>
    /// Сервис для управления маршрутизацией дронов.
    /// Предоставляет методы для получения и сохранения данных о дронах, маршрутах и гео-точках.
    /// </summary>
    public class RouteServices(AppDbContext context) : IRouteServices
    {
        private readonly AppDbContext _context = context;
        /// <summary>
        ///  Получает список всех дронов.
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список объектов дронов.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<List<Drone>> GetDroneAsync()
        {
            try
            {
                return await _context.Drons.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных списка всех дронов", ex);
            }
        }
        /// <summary>
        /// Получает дрон по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор дрона.</param>
        /// <returns>Асинхронная задача, возвращающая объект дрона с указанным идентификатором или <c>null</c>, если дрон не найден.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<Drone> GetDroneByIdAsync(int id)
        {
            try
            {
                return await _context.Drons.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных дрона по идентификатору", ex);
            }
        }
        /// <summary>
        /// Получает дрон по его серийному номеру.
        /// </summary>
        /// <param name="serial">Серийный номер дрона.</param>
        /// <returns>Асинхронная задача, возвращающая объект дрона с указанным серийным номером или <c>null</c>, если дрон не найден.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<Drone> GetDroneBySerialAsync(string serial)
        {
            try
            {
                return await _context.Drons.FirstOrDefaultAsync(x => x.Serial == serial);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных дрона по серийному номеру", ex);
            }
        }
        /// <summary>
        /// Получает список всех маршрутов дронов.
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список объектов маршрутов дронов.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<List<DroneRoute>> GetRouteDroneAsync()
        {
            try
            {
                return await _context.DroneRouts.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных списка всех маршрутов дрона", ex);
            }
        }
        /// <summary>
        /// Получает маршрут дрона по уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор маршрута дрона.</param>
        /// <returns>Асинхронная задача, возвращающая объект маршрута дрона с указанным идентификатором или <c>null</c>, если маршрут не найден.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<DroneRoute> GetRouteDroneByIdAsync(int id)
        {
            try
            {
                return await _context.DroneRouts.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных маршрута дрона по идентификатору", ex);
            }
        }
        /// <summary>
        /// Получает список всех гео-точек.
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список объектов гео-точек.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<List<Waypoint>> GetWaypointAsync()
        {
            try
            {
                return await _context.Waypoints.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных списка всех гео точек всех маршрутов", ex);
            }
        }
        /// <summary>
        /// Получает гео-точку по уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор гео-точки.</param>
        /// <returns>Асинхронная задача, возвращающая объект гео-точки с указанным идентификатором или <c>null</c>, если гео-точка не найдена.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<Waypoint> GetWaypointByIdAsync(int id)
        {
            try
            {
                return await _context.Waypoints.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки из базы данных гео точки по идентификатору", ex);
            }
        }
        /// <summary>
        /// Сохраняет новый дрон в базе данных.
        /// </summary>
        /// <param name="drone">Объект дрона, который необходимо сохранить.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если дрон успешно сохранен; в противном случае <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<bool> SaveDroneAsync(Drone drone)
        {
            try
            {
                _context.Drons.Add(drone);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка сохранения в базе данных карточки товара", ex);
            }
        }
        /// <summary>
        /// Сохраняет новый маршрут дрона в базе данных.
        /// </summary>
        /// <param name="droneRoute">Объект маршрута дрона, который необходимо сохранить.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если маршрут успешно сохранен; в противном случае <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<bool> SaveRouteDroneAsync(DroneRoute droneRoute)
        {
            try
            {
                _context.DroneRouts.Add(droneRoute);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка сохранения в базе данных маршрута", ex);
            }
        }
        /// <summary>
        /// Сохраняет новую гео-точку в базе данных.
        /// </summary>
        /// <param name="waypoint">Объект гео-точки, который необходимо сохранить.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если гео-точка успешно сохранена; в противном случае <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Исключение</exception>
        public async Task<bool> SaveWaypointAsync(Waypoint waypoint)
        {
            try
            {
                _context.Waypoints.Add(waypoint);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка сохранения в базе данных гео позиции маршрута", ex);
            }
        }
    }
}
