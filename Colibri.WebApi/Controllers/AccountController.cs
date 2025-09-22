using Colibri.Data.Entity;
using Colibri.Data.Helpers;
using Colibri.Data.Services.Abstracts;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Colibri.WebApi.Controllers
{
    /// <summary>
    /// Контроллер для управления учетными записями пользователей, включая регистрацию и аутентификацию.
    /// </summary>
    [Route("account")]
    [ApiController]
    public class AccountController(IAccountService account, IJwtGenerator jwtGenerator, ILoggerService logger) : Controller
    {
        private readonly IAccountService _account = account;
        private readonly ILoggerService _logger = logger;
        private readonly IJwtGenerator _jwtGenerator = jwtGenerator;
        /// <summary>
        /// Регистрирует нового пользователя с указанными данными.
        /// </summary>
        /// <param name="register">Объект, содержащий данные для регистрации пользователя с назначением роли. 
        /// Имена ролей:
        /// buyer - покупатель
        /// seller - продавец
        /// technician - техник</param>
        /// <returns>Результат регистрации. Возвращает <see cref="OkResult"/> при успешной регистрации или <see cref="BadRequestResult"/> при ошибке.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
            try
            {
                var user = new User
                {
                    UserName = register.Email,
                    NormalizedEmail = register.Email.ToUpper(),
                    Email = register.Email,
                    NormalizedUserName = register.Email.ToUpper(),
                    FirstName = register.FirstName ?? null,
                    LastName = register.LastName ?? null
                };

                var result = await _account.AddUserAsync(user, register.Password);

                if (result)
                {
                    if(!string.IsNullOrEmpty(register.Role))
                    {
                        var roleResult = await _account.RoleAssignmentAsync(user, register.Role);

                        if(roleResult)
                        {
                            _logger.LogMessage(User, $"Пользователь зарегистрирован с ролью: [{register.Role}]", LogLevel.Information);
                            return Ok(new { Message = $"Регистрация прошла успешно. Роль '{register.Role}' назначена." });
                        }
                        else
                        {
                            _logger.LogMessage(User, $"Пользователь зарегистрирован, но не удалось назначить роль: [{register.Role}]", LogLevel.Warning);
                            return Ok(new { Message = "Регистрация прошла успешно, но не удалось назначить роль." });
                        }
                    }
                    else
                    {
                        _logger.LogMessage(User, "Пользователь зарегистрирован без роли", LogLevel.Information);
                        return Ok(new { Message = "Регистрация прошла успешно" });
                    }
                }
                return BadRequest("Пользователь с таким именем уже существует");
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }
        /// <summary>
        /// Аутентифицирует пользователя с указанными данными.
        /// </summary>
        /// <param name="login">Объект, содержащий учетные данные для аутентификации пользователя</param>
        /// <returns>Результат аутентификации. Возвращает <see cref="OkObjectResult"/> с JWT-токеном при успешной аутентификации или <see cref="UnauthorizedResult"/> при ошибке.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            try
            {
                var user = await _account.GetByNameUserAsync(login.Email);

                if (user != null && await _account.CheckPasswordAsync(user, login.Password))
                {
                    var roles = await _account.GetUserRolesAsync(user);

                    var token = _jwtGenerator.Seed(user.UserName, roles);

                    _logger.LogMessage(User, $"Пользователь аутентифицирован - получен токен - [{token}]", LogLevel.Information);

                    return Ok(new { Token = token });
                }
                else
                {
                    return Unauthorized("Ошибка пользователя или пароля");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }
        /// <summary>
        /// Назначение роли выбранному пользователю
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="role">Имя роли</param>
        /// <returns>Результат назначения роли. Возвращает <see cref="OkObjectResult"/> если роль установлена или <see cref="NotFoundResult"/> при ошибке.</returns>
        [HttpPost("setroles")]
        [Authorize]
        public async Task<IActionResult> SetUserRole(string userId, string role)
        {
            try
            {
                var user = await _account.GetByIdUserAsync(Guid.Parse(userId));

                if (user == null) return BadRequest();

                var result = await _account.RoleAssignmentAsync(user, role);

                if (result)
                {
                    _logger.LogMessage(User, $"Пользователю назначена роль: [{role}]", LogLevel.Information);
                    return Ok(result);
                }
                else
                {
                    _logger.LogMessage(User, $"Роль: [{role}] уже назначена пользователю", LogLevel.Information);
                    return NotFound("Роль существует");
                }
            }
            catch(Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }
        /// <summary>
        /// Удаление роли у пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="role">Имя роли</param>
        /// <returns>Результат удаление роли. Возвращает <see cref="OkObjectResult"/> если роль удалена или <see cref="NotFoundResult"/> при ошибке.</returns>
        [HttpPost("canselrole")]
        [Authorize]
        public async Task<IActionResult> ConselRoleUser(string userId, string role)
        {
            try
            {
                var user = await _account.GetByIdUserAsync(Guid.Parse(userId));

                if (user == null) return BadRequest();

                var result = await _account.RoleCancelAsync(user, role);

                if (result)
                {
                    _logger.LogMessage(User, $"С пользователя снята роль: [{role}]", LogLevel.Information);
                    return Ok(result);
                }
                else
                {
                    _logger.LogMessage(User, $"Роли у пользователя не существует: [{role}]", LogLevel.Information);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }
        /// <summary>
        /// Получение всех пользователей
        /// </summary>
        /// <returns>Возвращает всех пользователей в формате json</returns>
        [HttpPost("users")]
       // [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _account.GetUsersAsync();

                if (users.Count == 0) return BadRequest();

                _logger.LogMessage(User, $"Выгружены пользователи в колличестве: [{users.Count}]", LogLevel.Information);

                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }
        /// <summary>
        /// Получение данных конкретного пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Возвращает данные пользователя в формате json</returns>
        [HttpPost("user")]
        [Authorize]
        public async Task<IActionResult> GetUser(string userId)
        {
            try
            {
                var user = await _account.GetByIdUserAsync(Guid.Parse(userId));

                if (user == null) return BadRequest();

                _logger.LogMessage(User, $"Выгружен пользователь с логином: [{user.UserName}]", LogLevel.Information);

                return Json(user);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }
        /// <summary>
        /// Создание роли
        /// </summary>
        /// <param name="role">Создаваемая роль</param>
        /// <returns>Результат создание роли. Возвращает <see cref="OkObjectResult"/> если роль создана или <see cref="BadRequestResult"/> при ошибке.</returns>
        [HttpPost("addrole")]
        [Authorize]
        public async Task<IActionResult> SetRole(Role role)
        {
            try
            {
                var result = await _account.AddRoleAsync(role);

                if (!result) return BadRequest();

                _logger.LogMessage(User, $"Создана роль с именем: [{role.Name}]", LogLevel.Information);

                return Ok("Роль создана");
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }
    }
}
