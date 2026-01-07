using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Colibri.WebApi.Services.Abstract;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.IO;

namespace Colibri.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DronBoxController : ControllerBase
    {
        private readonly IDroneConnectionService _droneConnectionService;
        private readonly ILogger<DronBoxController> _logger;
        
        public DronBoxController(
            IDroneConnectionService droneConnectionService,
            ILogger<DronBoxController> logger)
        {
            _droneConnectionService = droneConnectionService;
            _logger = logger;
        }

        /// <summary>
        /// Открытие крыши true, закрытие false
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        [HttpPost("roof")]
        public async Task<IActionResult> Roof(bool active)
        {
            try
            {
                _logger.LogInformation($"🏠 Roof command: {active}");
                
                var command = new
                {
                    command = "roof",
                    state = active,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                
                var result = await _droneConnectionService.SendCommandToDrone("box/control", command);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        status = "success", 
                        message = $"Roof {(active ? "opened" : "closed")} successfully",
                        drone_response = result
                    });
                }
                else
                {
<<<<<<< HEAD
                    return StatusCode(503, new 
                    { 
                        status = "error", 
                        message = $"Failed to control roof: {result.ErrorMessage}",
                        requested_state = active
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error controlling roof");
                return StatusCode(500, new 
                { 
                    status = "error", 
                    message = ex.Message 
                });
            }
        }

        /// <summary>
        /// Позиция - центр(true), край(false)
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        [HttpPost("position")]
        public async Task<IActionResult> Position(bool active)
        {
            try
            {
                _logger.LogInformation($"📍 Position command: {active}");
                
                var command = new
                {
                    command = "position_platform",
                    state = active,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                
                var result = await _droneConnectionService.SendCommandToDrone("box/control", command);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        status = "success", 
                        message = $"Position platform {(active ? "raised" : "lowered")}",
                        drone_response = result
                    });
                }
                else
                {
                    return StatusCode(503, new 
                    { 
                        status = "error", 
                        message = result.ErrorMessage,
                        requested_state = active
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error controlling position platform");
                return StatusCode(500, new 
                { 
                    status = "error", 
                    message = ex.Message 
                });
            }
        }

        /// <summary>
        /// Стол вверх(true), вниз(false) 
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        [HttpPost("table")]
        public async Task<IActionResult> Table(bool active)
        {
            try
            {
                _logger.LogInformation($"🛋️ Table command: {active}");
                
                var command = new
                {
                    command = "table",
                    state = active,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                
                var result = await _droneConnectionService.SendCommandToDrone("box/control", command);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        status = "success", 
                        message = $"Table {(active ? "extended" : "retracted")}",
                        drone_response = result
                    });
                }
                else
                {
                    return StatusCode(503, new 
                    { 
                        status = "error", 
                        message = result.ErrorMessage,
                        requested_state = active
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error controlling table");
                return StatusCode(500, new 
                { 
                    status = "error", 
                    message = ex.Message 
                });
            }
        }

        /// <summary>
        /// Люк открыть(true), закрыть(false)
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        [HttpPost("luke")]
        public async Task<IActionResult> Luke(bool active)
        {
            try
            {
                _logger.LogInformation($"🚪 Luke command: {active}");
                
                var command = new
                {
                    command = "luke",
                    state = active,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                
                var result = await _droneConnectionService.SendCommandToDrone("box/control", command);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        status = "success", 
                        message = $"Luke {(active ? "opened" : "closed")}",
                        drone_response = result
                    });
                }
                else
                {
                    return StatusCode(503, new 
                    { 
                        status = "error", 
                        message = result.ErrorMessage,
                        requested_state = active
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error controlling luke");
                return StatusCode(500, new 
                { 
                    status = "error", 
                    message = ex.Message 
                });
            }
        }        

        /// <summary>
        /// Аккум дрон установить(true), снять(false)
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        [HttpPost("drone_battery")]
        public async Task<IActionResult> DroneBattery(bool active)
        {
            try
            {
                _logger.LogInformation($"🔋 Drone battery command: {active}");
                
                var command = new
                {
                    command = "drone_battery",
                    state = active,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                
                var result = await _droneConnectionService.SendCommandToDrone("box/control", command);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        status = "success", 
                        message = $"Drone battery {(active ? "connected" : "disconnected")}",
                        drone_response = result
                    });
                }
                else
                {
                    return StatusCode(503, new 
                    { 
                        status = "error", 
                        message = result.ErrorMessage,
                        requested_state = active
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error controlling drone battery");
                return StatusCode(500, new 
                { 
                    status = "error", 
                    message = ex.Message 
                });
            }
        }    

        /// <summary>
        /// Запуск G-кода на станке LinuxCNC
        /// </summary>
        /// <param name="active">true - запустить G-код</param>
        /// <returns></returns>
        [HttpPost("run-gcode")]
        public async Task<IActionResult> RunGCode(bool active)
        {
            try
            {
                _logger.LogInformation($"🚀 Run G-code command: {active}");
                
                if (!active)
                {
                    return BadRequest(new 
                    { 
                        status = "error", 
                        message = "Для запуска G-кода укажите active=true"
                    });
                }
                
                // Создаем TCP клиент для подключения к Python серверу
                using var client = new TcpClient();
                
                // Параметры подключения (добавьте в appsettings.json)
                string serverIp = "85.141.101.22";
                int serverPort = 8888;
                
                try
                {
                    // Подключаемся к Python TCP серверу
                    await client.ConnectAsync(serverIp, serverPort);
                }
                catch (SocketException ex)
                {
                    _logger.LogError($"❌ Не удалось подключиться к LinuxCNC серверу {serverIp}:{serverPort}");
                    return StatusCode(503, new 
                    { 
                        status = "error", 
                        message = $"Cannot connect to LinuxCNC server: {ex.Message}",
                        suggestion = "Убедитесь, что Python скрипт с TCP сервером запущен"
                    });
                }
                
                using var stream = client.GetStream();
                using var writer = new StreamWriter(stream);
                using var reader = new StreamReader(stream);
                
                // Отправляем команду RUN
                await writer.WriteLineAsync("RUN");
                await writer.FlushAsync();
                
                // Читаем ответ от сервера
                string response = await reader.ReadLineAsync();
                
                _logger.LogInformation($"📥 Ответ от LinuxCNC: {response}");
                
                bool success = response?.StartsWith("OK:") == true;
                
                if (success)
                {
                    return Ok(new 
                    { 
                        status = "success", 
                        message = "G-code program executed successfully",
                        linuxcnc_response = response,
                        timestamp = DateTime.UtcNow.ToString("o")
                    });
                }
                else
                {
                    return StatusCode(503, new 
                    { 
                        status = "error", 
                        message = response ?? "No response from LinuxCNC server",
                        requested_state = active
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error running G-code program");
                return StatusCode(500, new 
                { 
                    status = "error", 
                    message = ex.Message 
=======
                    return BadRequest(new 
                    { 
                        success = false, 
                        error = response,
                        message = $"Ошибка при попытке {action} актуаторы"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = ex.Message,
                    message = "Ошибка управления актуаторами"
                });
            }
        }

        [HttpPost("position")]
        public async Task<IActionResult> SetPosition([FromBody] bool isCenter)
        {
            try
            {
                string command = isCenter ? "CENTER" : "EDGE";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    return Ok(new 
                    { 
                        success = true, 
                        message = isCenter ? "Перемещено в центр" : "Перемещено к краю",
                        isCenter = isCenter,
                        gcode = isCenter ? "G1 B284 C284 U284 V284 F1000" : "G1 B0 C0 U0 V0 F1000",
                        linuxcncResponse = response
                    });
                }
                else
                {
                    return BadRequest(new 
                    { 
                        success = false, 
                        error = response,
                        message = "Ошибка перемещения"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = ex.Message,
                    message = "Ошибка управления позицией"
                });
            }
        }

        [HttpPost("table")]
        public async Task<IActionResult> SetTable([FromBody] bool isUp)
        {
            try
            {
                string command = isUp ? "TABLEUP" : "TABLEDOWN";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    return Ok(new 
                    { 
                        success = true, 
                        message = isUp ? "Стол поднят" : "Стол опущен",
                        isUp = isUp,
                        gcode = isUp ? "G1 W200 F600" : "G1 W0 F600",
                        linuxcncResponse = response
                    });
                }
                else
                {
                    return BadRequest(new 
                    { 
                        success = false, 
                        error = response,
                        message = "Ошибка управления столом"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = ex.Message,
                    message = "Ошибка управления столом"
                });
            }
        }

        [HttpPost("luke")]
        public async Task<IActionResult> SetLuke([FromBody] bool isOpen)
        {
            try
            {
                string command = isOpen ? "LUKEN" : "LUKEO";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    return Ok(new 
                    { 
                        success = true, 
                        message = isOpen ? "Люк открыт" : "Люк закрыт",
                        isOpen = isOpen,
                        mcode = isOpen ? "M64 P1" : "M65 P1",
                        linuxcncResponse = response
                    });
                }
                else
                {
                    return BadRequest(new 
                    { 
                        success = false, 
                        error = response,
                        message = "Ошибка управления люком"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = ex.Message,
                    message = "Ошибка управления люком"
                });
            }
        }

        [HttpPost("drone_battery")]
        public async Task<IActionResult> SetDroneBattery([FromBody] bool isInstall)
        {
            try
            {
                string command = isInstall ? "DRONEPUTON" : "DRONETAKEOFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    return Ok(new 
                    { 
                        success = true, 
                        message = isInstall ? "Аккумулятор дрона установлен" : "Аккумулятор дрона снят",
                        isInstall = isInstall,
                        gcodeProgram = isInstall ? 
                            "G1 X7 Y259 F1000; G1 Z170 A20 F1000; G1 Z180 A0 F100; G1 Z0 F1000" :
                            "G1 X7 Y259 F1000; G1 Z175 F1000; G1 Z180 A5 F100; G1 Z170 A30 F100; G1 Z0 F1000",
                        linuxcncResponse = response
                    });
                }
                else
                {
                    return BadRequest(new 
                    { 
                        success = false, 
                        error = response,
                        message = "Ошибка управления аккумулятором дрона"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = ex.Message,
                    message = "Ошибка управления аккумулятором дрона"
                });
            }
        }

        [HttpPost("battery1")]
        public async Task<IActionResult> SetBattery1([FromBody] bool isInstall)
        {
            try
            {
                string command = isInstall ? "B1PUTON" : "B1TAKEOFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    return Ok(new 
                    { 
                        success = true, 
                        message = isInstall ? "Батарея 1 установлена" : "Батарея 1 снята",
                        isInstall = isInstall,
                        gcodeProgram = isInstall ? 
                            "G1 X7 Y511 F1000; G1 Z30 A20 F1000; G1 Z40 A0 F100; G1 Z0 F1000" :
                            "G1 Z35 F1000; G1 Z40 A5 F100; G1 Z30 A30 F100; G1 Z0 F1000",
                        linuxcncResponse = response
                    });
                }
                else
                {
                    return BadRequest(new 
                    { 
                        success = false, 
                        error = response,
                        message = "Ошибка управления батареей 1"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = ex.Message,
                    message = "Ошибка управления батареей 1"
                });
            }
        }

        [HttpPost("battery1_charger")]
        public async Task<IActionResult> SetBattery1Charger([FromBody] bool isOn)
        {
            try
            {
                string command = isOn ? "B1ON" : "B1OFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    return Ok(new 
                    { 
                        success = true, 
                        message = isOn ? "Зарядка батареи 1 включена" : "Зарядка батареи 1 выключена",
                        isOn = isOn,
                        mcode = isOn ? "M64 P3" : "M65 P3",
                        linuxcncResponse = response
                    });
                }
                else
                {
                    return BadRequest(new 
                    { 
                        success = false, 
                        error = response,
                        message = "Ошибка управления зарядкой батареи 1"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = ex.Message,
                    message = "Ошибка управления зарядкой батареи 1"
>>>>>>> 8399472 (Фиксация рабочая для тестирования)
                });
            }
        }

<<<<<<< HEAD
        /// <summary>
        /// Аккум 2
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        [HttpPost("battery2")]
        public async Task<IActionResult> Battery2(bool active)
        {
            try
            {
                _logger.LogInformation($"🔋 Battery 2 command: {active}");
                
                var command = new
                {
                    command = "battery2",
                    state = active,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                
                var result = await _droneConnectionService.SendCommandToDrone("box/batteries", command);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        status = "success", 
                        message = $"Battery 2 {(active ? "enabled" : "disabled")}",
                        drone_response = result
=======
        [HttpPost("battery2")]
        public async Task<IActionResult> SetBattery2([FromBody] bool isInstall)
        {
            try
            {
                string command = isInstall ? "B2PUTON" : "B2TAKEOFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    return Ok(new 
                    { 
                        success = true, 
                        message = isInstall ? "Батарея 2 установлена" : "Батарея 2 снята",
                        isInstall = isInstall,
                        gcodeProgram = isInstall ? 
                            "G1 X241 Y259 F1000; G1 Z30 A20 F1000; G1 Z40 A0 F100; G1 Z0 F1000" :
                            "G1 Z35 F1000; G1 Z40 A5 F100; G1 Z30 A30 F100; G1 Z0 F1000",
                        linuxcncResponse = response
>>>>>>> 8399472 (Фиксация рабочая для тестирования)
                    });
                }
                else
                {
<<<<<<< HEAD
                    return StatusCode(503, new 
                    { 
                        status = "error", 
                        message = result.ErrorMessage,
                        requested_state = active
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error controlling battery 2");
                return StatusCode(500, new 
                { 
                    status = "error", 
                    message = ex.Message 
                });
            }
        }  

        /// <summary>
        /// Аккум 3
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        [HttpPost("battery3")]
        public async Task<IActionResult> Battery3(bool active)
        {
            try
            {
                _logger.LogInformation($"🔋 Battery 3 command: {active}");
                
                var command = new
                {
                    command = "battery3",
                    state = active,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                
                var result = await _droneConnectionService.SendCommandToDrone("box/batteries", command);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        status = "success", 
                        message = $"Battery 3 {(active ? "enabled" : "disabled")}",
                        drone_response = result
                    });
                }
                else
                {
                    return StatusCode(503, new 
                    { 
                        status = "error", 
                        message = result.ErrorMessage,
                        requested_state = active
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error controlling battery 3");
                return StatusCode(500, new 
                { 
                    status = "error", 
                    message = ex.Message 
=======
                    return BadRequest(new 
                    { 
                        success = false, 
                        error = response,
                        message = "Ошибка управления батареей 2"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = ex.Message,
                    message = "Ошибка управления батареей 2"
>>>>>>> 8399472 (Фиксация рабочая для тестирования)
                });
            }
        }

<<<<<<< HEAD
        /// <summary>
        /// Стоп (true)
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        [HttpPost("stop")]
        public async Task<IActionResult> Stop(bool active)
        {
            try
            {
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetBoxStatus()
        {
            try
            {
                _logger.LogInformation("📊 Getting box status");
                
                // Запрашиваем статус бокса у дрона
                var result = await _droneConnectionService.SendCommandToDrone("box/status", null);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        status = "success", 
                        message = "Box status retrieved",
                        data = result
                    });
                }
                else
                {
                    return StatusCode(503, new 
                    { 
                        status = "error", 
                        message = "Failed to get box status",
                        error = result.ErrorMessage
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error getting box status");
                return StatusCode(500, new 
                { 
                    status = "error", 
                    message = ex.Message 
                });
            }
        }
    }
}
=======
        [HttpPost("battery2_charger")]
        public async Task<IActionResult> SetBattery2Charger([FromBody] bool isOn)
        {
            try
            {
                string command = isOn ? "B2ON" : "B2OFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    return Ok(new 
                    { 
                        success = true, 
                        message = isOn ? "Зарядка батареи 2 включена" : "Зарядка батареи 2 выключена",
                        isOn = isOn,
                        mcode = isOn ? "M64 P4" : "M65 P4",
                        linuxcncResponse = response
                    });
                }
                else
                {
                    return BadRequest(new 
                    { 
                        success = false, 
                        error = response,
                        message = "Ошибка управления зарядкой батареи 2"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = ex.Message,
                    message = "Ошибка управления зарядкой батареи 2"
                });
            }
        }

        [HttpPost("battery3")]
        public async Task<IActionResult> SetBattery3([FromBody] bool isInstall)
        {
            try
            {
                string command = isInstall ? "B3PUTON" : "B3TAKEOFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    return Ok(new 
                    { 
                        success = true, 
                        message = isInstall ? "Батарея 3 установлена" : "Батарея 3 снята",
                        isInstall = isInstall,
                        gcodeProgram = isInstall ? 
                            "G1 X7 Y4 F1000; G1 Z30 A20 F1000; G1 Z40 A0 F100; G1 Z0 F1000" :
                            "G1 Z35 F1000; G1 Z40 A5 F100; G1 Z30 A30 F100; G1 Z0 F1000",
                        linuxcncResponse = response
                    });
                }
                else
                {
                    return BadRequest(new 
                    { 
                        success = false, 
                        error = response,
                        message = "Ошибка управления батареей 3"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = ex.Message,
                    message = "Ошибка управления батареей 3"
                });
            }
        }

        [HttpPost("battery3_charger")]
        public async Task<IActionResult> SetBattery3Charger([FromBody] bool isOn)
        {
            try
            {
                string command = isOn ? "B3ON" : "B3OFF";
                string response = await SendToLinuxCNC(command);
                
                if (response.StartsWith("OK:"))
                {
                    return Ok(new 
                    { 
                        success = true, 
                        message = isOn ? "Зарядка батареи 3 включена" : "Зарядка батареи 3 выключена",
                        isOn = isOn,
                        mcode = isOn ? "M64 P5" : "M65 P5",
                        linuxcncResponse = response
                    });
                }
                else
                {
                    return BadRequest(new 
                    { 
                        success = false, 
                        error = response,
                        message = "Ошибка управления зарядкой батареи 3"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = ex.Message,
                    message = "Ошибка управления зарядкой батареи 3"
                });
            }
        }

        [HttpPost("stop")]
        public IActionResult Stop([FromBody] bool stop)
        {
            // Заглушка для команды стоп
            if (stop)
            {
                return Ok("Стоп команда выполнена");
            }
            return BadRequest("Для команды стоп требуется значение true");
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            // Заглушка для получения статуса
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
            
            return Ok(status);
        }

        private async Task<string> SendToLinuxCNC(string command)
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
>>>>>>> 8399472 (Фиксация рабочая для тестирования)
