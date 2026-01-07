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
                });
            }
        }

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
                });
            }
        }

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