using Colibri.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace Colibri.Data.Context
{
    /// <summary>
    /// Основной класс работы с базой данных
    /// </summary>
    public class AppDbContext : AppDbContextBase, IAppDbContext
    {
        /// <summary>
        /// Конструктор контекста базы данных
        /// </summary>
        /// <param name="options">Настройки подключения к базе дыннх</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        /// <summary>
        /// Сущность логов
        /// </summary>
        public DbSet<Log> Logger { get; set; }
        /// <summary>
        /// Сущность товара
        /// </summary>
        public DbSet<Product> Products { get; set; }
        /// <summary>
        /// Сущность карточки товара
        /// </summary>
        public DbSet<Order> Orders { get; set; }
        /// <summary>
        /// Сущность дрона
        /// </summary>
        public DbSet<Drone> Drons { get; set; }
        /// <summary>
        /// Сущность точек гео позиции заказа
        /// </summary>
        public DbSet<Waypoint> Waypoints { get; set; }
        /// <summary>
        /// Сущность маршрута дрона
        /// </summary>
        public DbSet<DroneRoute> DroneRouts { get; set; }
        /// <summary>
        /// Сущность состояния события
        /// </summary>
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        /// <summary>
        /// Сущность событий
        /// </summary>
        public DbSet<Event> Events { get; set; }
        /// <summary>
        /// Сущность телеметрии
        /// </summary>
        public DbSet<Telemetry> Telemetries { get; set; }
    }
}
