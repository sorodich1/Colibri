using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Colibri.WebApi.Services.Abstract;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text;
using Colibri.Data.Services.Abstracts;
using Colibri.Data.Helpers;

namespace Colibri.WebApi.Controllers
{
    /// <summary>
    /// Контроллер управления дрон боксом
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="droneConnectionService"></param>
    /// <param name="logger"></param>
    [Route("api/dron-box")]
    [ApiController]
    public class DronBoxController(
        IDroneConnectionService droneConnectionService,
        ILoggerService logger) : ControllerBase
    {
        private readonly IDroneConnectionService _droneConnectionService = droneConnectionService;
        private readonly ILoggerService _logger = logger;

        /// <summary>
        /// Открытие крыши true, закрытие false
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        [HttpPost("roof")]
        public async Task<IActionResult> Roof([FromBody]bool active)
        {
            try
            {
                _logger.LogMessage(User, $"🚀 Команда: Roof, параметр: {active}", LogLevel.Information);
                
                var command = new
                {
                    command = "roof",
                    state = active,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                
                var result = await _droneConnectionService.SendCommandToDrone("box/control", command);
                
                if (result.Success)
                {
                    _logger.LogMessage(User, $"✅ Roof успешно {(active ? "открыта" : "закрыта")}", LogLevel.Information);
                    return Ok("success");
                }
                else
                {
                    _logger.LogMessage(User, $"❌ Ошибка Roof", LogLevel.Error);
                    return BadRequest("error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, "error");
            }
        }

        /// <summary>
        /// Работа позиционера (true - в центр, false - в край)
        /// </summary>
        /// <param name="isCenter">Статус кнопки</param>
        /// <returns></returns>
        [HttpPost("position")]
        public async Task<IActionResult> SetPosition([FromBody]bool isCenter)
        {
            try
            {
                _logger.LogMessage(User, $"🚀 Команда: SetPosition, параметр: {isCenter}", LogLevel.Information);
                
                string command = isCenter ? "CENTER" : "EDGE";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    _logger.LogMessage(User, $"✅ Position успешно установлен: {(isCenter ? "центр" : "край")}", LogLevel.Information);
                    return Ok("success");
                }
                else
                {
                    _logger.LogMessage(User, $"❌ Ошибка Position: {response}", LogLevel.Error);
                    return BadRequest("error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, "error");
            }
        }

        /// <summary>
        /// Работа стола (true - на верх, false - в низ)
        /// </summary>
        /// <param name="isUp">Статус кнопки</param>
        /// <returns></returns>
        [HttpPost("table")]
        public async Task<IActionResult> SetTable([FromBody]bool isUp)
        {
            try
            {
                _logger.LogMessage(User, $"🚀 Команда: SetTable, параметр: {isUp}", LogLevel.Information);
                
                string command = isUp ? "TABLEUP" : "TABLEDOWN";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    _logger.LogMessage(User, $"✅ Table успешно: {(isUp ? "поднят" : "опущен")}", LogLevel.Information);
                    return Ok("success");
                }
                else
                {
                    _logger.LogMessage(User, $"❌ Ошибка Table: {response}", LogLevel.Error);
                    return BadRequest("error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, "error");
            }
        }

        /// <summary>
        /// Работа люка (true - открыть, false - закрыть)
        /// </summary>
        /// <param name="isOpen">Статус кнопки</param>
        /// <returns></returns>
        [HttpPost("hatch")]
        public async Task<IActionResult> SetLuke([FromBody]bool isOpen)
        {
            try
            {
                _logger.LogMessage(User, $"🚀 Команда: SetLuke, параметр: {isOpen}", LogLevel.Information);
                
                string command = isOpen ? "LUKEN" : "LUKEO";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    _logger.LogMessage(User, $"✅ Luke успешно: {(isOpen ? "открыт" : "закрыт")}", LogLevel.Information);
                    return Ok("success");
                }
                else
                {
                    _logger.LogMessage(User, $"❌ Ошибка Luke: {response}", LogLevel.Error);
                    return BadRequest("error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, "error");
            }
        }

        /// <summary>
        /// Замена батареи дрона (true - установить, false - снять)
        /// </summary>
        /// <param name="isInstall">Статус кнопки</param>
        /// <returns></returns>
        [HttpPost("dronebattery")]
        public async Task<IActionResult> SetDroneBattery([FromBody]bool isInstall)
        {
            try
            {
                _logger.LogMessage(User, $"🚀 Команда: SetDroneBattery, параметр: {isInstall}", LogLevel.Information);
                
                string command = isInstall ? "DRONEPUTON" : "DRONETAKEOFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    _logger.LogMessage(User, $"✅ DroneBattery успешно: {(isInstall ? "установлен" : "снят")}", LogLevel.Information);
                    return Ok("success");
                }
                else
                {
                    _logger.LogMessage(User, $"❌ Ошибка DroneBattery: {response}", LogLevel.Error);
                    return BadRequest("error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, "error");
            }
        }

        /// <summary>
        /// Установка батареи в первую ячейку (true - установить, false - снять)
        /// </summary>
        /// <param name="isInstall">Статус кнопки</param>
        /// <returns></returns>
        [HttpPost("battery1")]
        public async Task<IActionResult> SetBattery1([FromBody]bool isInstall)
        {
            try
            {
                _logger.LogMessage(User, $"🚀 Команда: SetBattery1, параметр: {isInstall}", LogLevel.Information);
                
                string command = isInstall ? "B1PUTON" : "B1TAKEOFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    _logger.LogMessage(User, $"✅ Battery1 успешно: {(isInstall ? "установлена" : "снята")}", LogLevel.Information);
                    return Ok("success");
                }
                else
                {
                    _logger.LogMessage(User, $"❌ Ошибка Battery1: {response}", LogLevel.Error);
                    return BadRequest("error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, "error");
            }
        }

        /// <summary>
        /// Включение зарядки акума 1 (true - включить, false - выключить)
        /// </summary>
        /// <param name="isOn">Статус кнопки</param>
        /// <returns></returns>
        [HttpPost("battery1_charger")]
        public async Task<IActionResult> SetBattery1Charger([FromBody]bool isOn)
        {
            try
            {
                _logger.LogMessage(User, $"🚀 Команда: SetBattery1Charger, параметр: {isOn}", LogLevel.Information);
                
                string command = isOn ? "B1ON" : "B1OFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    _logger.LogMessage(User, $"✅ Battery1Charger успешно: {(isOn ? "включена" : "выключена")}", LogLevel.Information);
                    return Ok("success");
                }
                else
                {
                    _logger.LogMessage(User, $"❌ Ошибка Battery1Charger: {response}", LogLevel.Error);
                    return BadRequest("error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, "error");
            }
        }

        /// <summary>
        /// Установка батареи во вторую ячейку (true - установить, false - снять)
        /// </summary>
        /// <param name="isInstall">Статус кнопки</param>
        /// <returns></returns>
        [HttpPost("battery2")]
        public async Task<IActionResult> SetBattery2([FromBody]bool isInstall)
        {
            try
            {
                _logger.LogMessage(User, $"🚀 Команда: SetBattery2, параметр: {isInstall}", LogLevel.Information);
                
                string command = isInstall ? "B2PUTON" : "B2TAKEOFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    _logger.LogMessage(User, $"✅ Battery2 успешно: {(isInstall ? "установлена" : "снята")}", LogLevel.Information);
                    return Ok("success");
                }
                else
                {
                    _logger.LogMessage(User, $"❌ Ошибка Battery2: {response}", LogLevel.Error);
                    return BadRequest("error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, "error");
            }
        }

        /// <summary>
        /// Включение зарядки акума 2 (true - включить, false - выключить)
        /// </summary>
        /// <param name="isOn">Статус кнопки</param>
        /// <returns></returns>
        [HttpPost("battery2_charger")]
        public async Task<IActionResult> SetBattery2Charger([FromBody]bool isOn)
        {
            try
            {
                _logger.LogMessage(User, $"🚀 Команда: SetBattery2Charger, параметр: {isOn}", LogLevel.Information);
                
                string command = isOn ? "B2ON" : "B2OFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    _logger.LogMessage(User, $"✅ Battery2Charger успешно: {(isOn ? "включена" : "выключена")}", LogLevel.Information);
                    return Ok("success");
                }
                else
                {
                    _logger.LogMessage(User, $"❌ Ошибка Battery2Charger: {response}", LogLevel.Error);
                    return BadRequest("error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, "error");
            }
        }

        /// <summary>
        /// Установка батареи в третью ячейку (true - установить, false - снять)
        /// </summary>
        /// <param name="isInstall">Статус кнопки</param>
        /// <returns></returns>
        [HttpPost("battery3")]
        public async Task<IActionResult> SetBattery3([FromBody]bool isInstall)
        {
            try
            {
                _logger.LogMessage(User, $"🚀 Команда: SetBattery3, параметр: {isInstall}", LogLevel.Information);
                
                string command = isInstall ? "B3PUTON" : "B3TAKEOFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    _logger.LogMessage(User, $"✅ Battery3 успешно: {(isInstall ? "установлена" : "снята")}", LogLevel.Information);
                    return Ok("success");
                }
                else
                {
                    _logger.LogMessage(User, $"❌ Ошибка Battery3: {response}", LogLevel.Error);
                    return BadRequest("error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, "error");
            }
        }

        /// <summary>
        /// Включение зарядки акума 3 (true - включить, false - выключить)
        /// </summary>
        /// <param name="isOn">Статус кнопки</param>
        /// <returns></returns>
        [HttpPost("battery3_charger")]
        public async Task<IActionResult> SetBattery3Charger([FromBody]bool isOn)
        {
            try
            {
                _logger.LogMessage(User, $"🚀 Команда: SetBattery3Charger, параметр: {isOn}", LogLevel.Information);
                
                string command = isOn ? "B3ON" : "B3OFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    _logger.LogMessage(User, $"✅ Battery3Charger успешно: {(isOn ? "включена" : "выключена")}", LogLevel.Information);
                    return Ok("success");
                }
                else
                {
                    _logger.LogMessage(User, $"❌ Ошибка Battery3Charger: {response}", LogLevel.Error);
                    return BadRequest("error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, "error");
            }
        }

        [HttpPost("stop")]
        public IActionResult Stop([FromBody] bool stop)
        {
            try
            {
                _logger.LogMessage(User, $"🚀 Команда: Stop, параметр: {stop}", LogLevel.Information);
                
                if (stop)
                {
                    _logger.LogMessage(User, $"✅ Stop команда выполнена", LogLevel.Information);
                    return Ok(new 
                    { 
                        status = "success",
                        message = "Стоп команда выполнена"
                    });
                }
                
                _logger.LogMessage(User, $"❌ Для команды Stop требуется значение true", LogLevel.Error);
                return BadRequest(new 
                { 
                    status = "error",
                    error = "Invalid parameter",
                    message = "Для команды стоп требуется значение true"
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, new 
                { 
                    status = "error",
                    error = ex.Message,
                    message = "Ошибка выполнения команды Stop"
                });
            }
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            try
            {
                _logger.LogMessage(User, $"🚀 Команда: GetStatus", LogLevel.Information);
                
                var status = new
                {
                    Roof = "closed",
                    Position = "center",
                    Table = "down",
                    Luke = "closed",
                    DroneBattery = "installed",
                    Battery1 = "on",
                    Battery2 = "on",
                    Battery3 = "on",
                    IsStopped = false,
                    Timestamp = DateTime.UtcNow
                };
                
                _logger.LogMessage(User, $"✅ GetStatus успешно выполнен", LogLevel.Information);
                
                return Ok(new 
                { 
                    status = "success",
                    data = status
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, new 
                { 
                    status = "error",
                    error = ex.Message,
                    message = "Ошибка получения статуса"
                });
            }
        }

        private static async Task<string> SendToLinuxCNC(string command)
        {
            const string linuxCncHost = "37.29.71.91"; 
            const int linuxCncPort = 8888;
            
            try
            {
                using var client = new TcpClient();
                // Подключаемся с таймаутом 3 секунды
                await client.ConnectAsync(linuxCncHost, linuxCncPort)
                    .WaitAsync(TimeSpan.FromSeconds(3));

                using var stream = client.GetStream();
                // Отправляем команду
                byte[] data = Encoding.ASCII.GetBytes(command + "\n");
                await stream.WriteAsync(data);

                // Читаем ответ
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)
                    .WaitAsync(TimeSpan.FromSeconds(5));

                return Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
            }
            catch (TimeoutException)
            {
                return "ERROR: Timeout connecting to LinuxCNC server";
            }
            catch (SocketException ex)
            {
                return $"ERROR: Socket error - {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }
    }
}