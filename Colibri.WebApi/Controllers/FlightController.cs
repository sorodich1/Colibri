using Colibri.ConnectNetwork.Data;
using Colibri.ConnectNetwork.Services.Abstract;
using Colibri.Data.Entity;
using Colibri.Data.Helpers;
using Colibri.Data.Services.Abstracts;
using Colibri.GetDirection;
using Colibri.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Colibri.WebApi.Controllers
{
    /// <summary>
    /// Контроллер управления полётами. 
    /// </summary>
    [Route("flight")]
    [ApiController]
    public class FlightController(IHttpConnectService connect, ILoggerService logger, IConfiguration configuration, IFlightService flightServece) : Controller
    {
        private readonly IHttpConnectService _connect = connect;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILoggerService _logger = logger;
        private readonly IFlightService _flightServece = flightServece;

        /// <summary>
        /// Передача гео точек
        /// </summary>
        /// <returns></returns>
        [HttpPost("orderlocation")]
        public async Task<IActionResult> GeodataTransfer([FromBody] OrderLocation routeResponse)
        {
            string jsonDronBox = @"
            {
                ""Lat"": ""55.7558"",
                ""Lon"": ""37.6173"",
                ""Alt"": ""150"",
                ""Speed"": ""45"",
                ""Course"": ""180"",
                ""Sats"": ""8"",
                
                ""FixQuality"": ""1"",
                ""Hdop"": ""0.8"",
                ""Timestamp"": ""2024-04-27T12:34:56Z""
            }";

            // await _connect.GetAsync(_configuration["Url:DronBox"]);

            var gps = JsonConvert.DeserializeObject<GpsJson>(jsonDronBox);

            var json = await DirectionJson.RouteFly(routeResponse.SellerPoint, routeResponse.BuyerPoint);

            var jsonMission = await DirectionJson.MissionFile(json);

            return Ok(jsonMission);
        }

        /// <summary>
        /// Открытие бокса дрона
        /// </summary>
        /// <param name="isActive">true - открыть бокс, false - закрыть бокс</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("openbox")]
        public async Task<IActionResult> OpenBox(bool isActive)
        {
            try
            {
                EventRegistration registration = new()
                {
                    EventId = 1,
                    IsActive = isActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                await _flightServece.AddEventRegistration(registration);

                return Ok("success");
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }
    }
}