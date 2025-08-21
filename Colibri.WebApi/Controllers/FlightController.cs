using Colibri.ConnectNetwork.Data;
using Colibri.ConnectNetwork.Services;
using Colibri.GetDirection;
using Colibri.GetDirection.Data;
using Colibri.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Colibri.WebApi.Controllers
{
    /// <summary>
    /// Контроллер управления полётами. 
    /// </summary>
    [Route("flight")]
    [ApiController]
    public class FlightController(HttpConnectService connect, IConfiguration configuration) : Controller
    {
        private readonly HttpConnectService _connect = connect;
        private readonly IConfiguration _configuration = configuration;
        /// <summary>
        /// Передача гео точек
        /// </summary>
        /// <returns></returns>
        [HttpPost("orderlocation")]
        public async Task<IActionResult> GeodataTransfer([FromBody] OrderLocation routeResponse)
        {
            string jsonDronBox = await _connect.GetAsync(_configuration["Url:DronBox"]);

            var gps = JsonConvert.DeserializeObject<GpsJson>(jsonDronBox);

            var json = await DirectionJson.RouteFly(routeResponse.SellerPoint, routeResponse.BuyerPoint);

            var jsonMission = await DirectionJson.MissionFile(json);

            return Ok(jsonMission);
        }
    }
}
