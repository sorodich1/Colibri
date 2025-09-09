using Colibri.Data.Context;
using Colibri.Data.Entity;
using Colibri.Data.Services.Abstracts;
using System;
using System.Threading.Tasks;

namespace Colibri.Data.Services
{
    /// <summary>
    /// Реализация сервиса для работы с полетными путевыми точками.
    /// Предоставляет методы для добавления и управления путевыми точками в базе данных.
    /// </summary>
    /// <param name="context">Контекст базы данных для взаимодействия с сущностями.</param>
    public class FlightService(AppDbContext context) : IFlightService
    {
        private readonly AppDbContext _context = context;
        /// <summary>
        /// Асинхронно добавляет новую регистрацию события в базу данных.
        /// </summary>
        /// <param name="eventReg">бъект регистрации события, который нужно сохранить.</param>
        /// <returns>Задача, представляющая асинхронную операцию добавления регистрации.</returns>
        /// <exception cref="InvalidOperationException">Выбрасывается при возникновении ошибок при сохранении данных в базе данных, с оригинальным исключением в качестве внутреннего.</exception>
        public async Task AddEventRegistration(EventRegistration eventReg)
        {
            try
            {
                _context.EventRegistrations.Add(eventReg);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка сохранения в базе данных события", ex);
            }
        }

        /// <summary>
        /// Добавляет новую путевую точку (Waypoint) в базу данных.
        /// </summary>
        /// <param name="waypoint">Объект путевой точки, который нужно сохранить.</param>
        /// <returns>Задача, которая завершится после выполнения операции сохранения.</returns>
        /// <exception cref="InvalidOperationException">Выбрасывается при ошибке сохранения данных в базу.</exception>
        public async Task AddFlightWaipoints(Waypoint waypoint)
        {
            try
            {
                _context.Waypoints.Add(waypoint);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка сохранения в базе данных карточки товара", ex);
            }
        }
    }
}
