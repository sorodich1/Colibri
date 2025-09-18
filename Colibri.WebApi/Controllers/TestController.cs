using System;
using System.Threading.Tasks;
using Colibri.Data.Entity;
using Colibri.Data.Helpers;
using Colibri.Data.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Colibri.WebApi.Controllers
{
    /// <summary>
    /// Тестовый контроллер для роли "Техник"
    /// </summary>
    /// <param name="logger">Логирование данных</param>
    /// <param name="flightServece">Полётный сервис</param>
    [Route("test")]
    [ApiController]
    public class TestController(ILoggerService logger, IFlightService flightServece) : ControllerBase
    {
        private readonly ILoggerService _logger = logger;
        private readonly IFlightService _flightServece = flightServece;

        /// <summary>
        /// Тестовый метод взлёта на определённую высоту
        /// </summary>
        /// <param name="isActive">Работа дрона true - взлёт, false - посадка</param>
        /// <param name="distance">Высота взлёта в метрах</param>
        /// <returns>success - запрос выполнен, error - запрос не выполнен</returns>
        [Authorize]
        [HttpPost("SystemCheck")]
        public async Task<IActionResult> SystemCheck(bool isActive, int distance)
        {
            try
            {
                EventRegistration registration = new()
                {
                    EventId = 2,
                    IsActive = isActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                await _flightServece.AddEventRegistration(registration);

                return Ok("success");
            }
            catch(Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok("error");
            }
        }

        /// <summary>
        /// Тестирование подсветки дрона
        /// </summary>
        /// <param name="colorNumber">Номер цвета -- 0 - цвет выключен, 1 - зелёный, 2 - крассный</param>
        /// <returns>success - запрос выполнен, error - запрос не выполнен</returns>
        [Authorize]
        [HttpPost("BacklightTesting")]
        public async Task<IActionResult> BacklightTesting(int colorNumber)
        {
            try
            {
                _logger.LogMessage(User, $"Выбран цвет с номером {colorNumber}", LogLevel.Information);

                return Ok("success");
            }
            catch(Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok("error");
            }
        }

        /// <summary>
        /// Тестирование автопилота по заданным координатам
        /// </summary>
        /// <param name="latitude">Широта</param>
        /// <param name="longitude">Долгота</param>
        /// <returns>success - запрос выполнен, error - запрос не выполнен</returns>
        [Authorize]
        [HttpPost("TestAutopilot")]
        public async Task<IActionResult> TestAutopilot(double latitude, double longitude)
        {
            try
            {
                _logger.LogMessage(User, $"Тестируется полёт по координатам широта - {latitude}, долгота - {longitude}", LogLevel.Information);

                return Ok("success");
            }
            catch(Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok("error");
            }
        }
    }
}
