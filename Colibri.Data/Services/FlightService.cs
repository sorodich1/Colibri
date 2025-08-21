using Colibri.Data.Context;
using Colibri.Data.Entity;
using Colibri.Data.Services.Abstracts;
using System;
using System.Threading.Tasks;

namespace Colibri.Data.Services
{
    public class FlightService(AppDbContext context) : IFlightService
    {
        private readonly AppDbContext _context = context;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="waypoint"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task AddFlightWaipoints(Waypoint waypoint)
        {
            try
            {
                _context.Waypoints.Add(waypoint);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Ошибка сохранения в базе данных карточки товара", ex);
            }
        }
    }
}
