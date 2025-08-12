using Colibri.GetDirection;
using Colibri.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Colibri.WebApi.Controllers
{
    /// <summary>
    /// Контроллер управления полётами. 
    /// </summary>
    [Route("flight")]
    [ApiController]
    public class FlightController : Controller
    {
        /// <summary>
        /// Передача гео точек
        /// </summary>
        /// <returns></returns>
        [HttpPost("orderlocation")]
        public IActionResult GeodataTransfer([FromBody] OrderLocation routeResponse)
        {
            var json = DirectionJson.RouteFly(routeResponse.SellerPoint, routeResponse.BuyerPoint);

            return View(json);
        }
    }
}
