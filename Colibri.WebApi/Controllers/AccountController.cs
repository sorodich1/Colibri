using Colibri.Data.Entity;
using Colibri.Data.Helpers;
using Colibri.Data.Services.Abstracts;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colibri.WebApi.Controllers
{
    /// <summary>
    /// Контроллер для управления учетными записями пользователей, включая регистрацию и аутентификацию.
    /// </summary>
    [Route("account")]
    [ApiController]
    public class AccountController(IAccountService account, IJwtGenerator jwtGenerator, UserManager<User> manager, ILogger<AccountController> logger) : Controller
    {
        private readonly IAccountService _account = account;
        private readonly IJwtGenerator _jwtGenerator = jwtGenerator;
        private readonly UserManager<User> _manager = manager;
        private readonly ILogger<AccountController> _logger = logger;
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
                    LastName = register.LastName ?? null,
                    PhoneNumber = register.PhoneNumber ?? null
                };

                var result = await _account.AddUserAsync(user, register.Password);

                if (result)
                {
                    if(!string.IsNullOrEmpty(register.Role))
                    {
                        var roleResult = await _account.RoleAssignmentAsync(user, register.Role);

                        if(roleResult)
                        {
                            _logger.LogInformation($"Пользователь зарегистрирован с ролью: [{register.Role}]");
                            return Ok(new { Message = $"Регистрация прошла успешно. Роль '{register.Role}' назначена." });
                        }
                        else
                        {
                            _logger.LogWarning($"Пользователь зарегистрирован, но не удалось назначить роль: [{register.Role}]");
                            return Ok(new { Message = "Регистрация прошла успешно, но не удалось назначить роль." });
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Пользователь зарегистрирован без роли");
                        return Ok(new { Message = "Регистрация прошла успешно" });
                    }
                }
                return BadRequest("Пользователь с таким именем уже существует");
            }
            catch (Exception ex)
            {
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


                    return Ok(new { Token = token });
                }
                else
                {
                    return Unauthorized("Ошибка пользователя или пароля");
                }
            }
            catch (Exception ex)
            {
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
       // [Authorize]
        public async Task<IActionResult> SetUserRole(string userId, string role)
        {
            try
            {
                var user = await _account.GetByIdUserAsync(Guid.Parse(userId));

                if (user == null) return BadRequest();

                var result = await _account.RoleAssignmentAsync(user, role);

                if (result)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound("Роль существует");
                }
            }
            catch(Exception ex)
            {
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }

        /// <summary>
        /// Получение ролей пользователя по email или имени пользователя
        /// </summary>
        /// <param name="userIdentifier">Email или имя пользователя</param>
        /// <returns>Список ролей пользователя</returns>
        [HttpGet("user-roles-by-identifier")]
        [Authorize]
        public async Task<IActionResult> GetUserRolesByIdentifier([FromQuery] string userIdentifier)
        {
            try
            {
                if (string.IsNullOrEmpty(userIdentifier))
                {
                    return BadRequest(new { error = "Не указан идентификатор пользователя" });
                }

                User user;
                
                // Пытаемся найти по email
                user = await _account.GetByNameUserAsync(userIdentifier);
                
                // Если не нашли по email, пытаемся найти по UserName
                if (user == null)
                {
                    user = await _manager.FindByNameAsync(userIdentifier);
                }

                if (user == null)
                {
                    return NotFound(new { error = "Пользователь не найден" });
                }

                var roles = await _account.GetUserRolesAsync(user);
                
                return Ok(new
                {
                    userId = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    roles = roles.ToList()
                });
            }
            catch (Exception ex)
            {
               // _logger.LogMessage(User, $"Ошибка получения ролей пользователя: {ex.Message}", LogLevel.Error);
                return StatusCode(500, new { error = "Ошибка сервера", message = ex.Message });
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
                    return Ok(result);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
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


                return Json(users);
            }
            catch (Exception ex)
            {
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }
        /// <summary>
        /// Получение данных конкретного пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Возвращает данные пользователя в формате json</returns>
       // [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUser(string userId)
        {
            try
            {
                var user = await _account.GetByIdUserAsync(Guid.Parse(userId));

                if (user == null) 
                {
                    return Json(new { error = "Пользователь не найден" });
                }

                // Получаем роли пользователя
                var roles = await _account.GetUserRolesAsync(user);
                
                // Создаем DTO для ответа
                var userDto = new 
                {
                    id = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    surName = user.SurName,
                    lastLogin = user.LastLogin,
                    emailConfirmed = user.EmailConfirmed,
                    phoneNumberConfirmed = user.PhoneNumberConfirmed,
                    twoFactorEnabled = user.TwoFactorEnabled,
                    lockoutEnabled = user.LockoutEnabled,
                    accessFailedCount = user.AccessFailedCount,
                    phoneNumber = user.PhoneNumber,
                    roles = roles.ToList() // Преобразуем в список
                };


                return Json(userDto); // Теперь это чистый объект без циклических ссылок
            }
            catch (Exception ex)
            {
                
                // Возвращаем JSON с ошибкой, а не строку
                return Json(new { 
                    error = "Ошибка сервера", 
                    message = ex.Message 
                });
            }
        }

        /// <summary>
        /// Выборка пользователя по имени
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <returns></returns>
        [HttpPost("userName")]
        [Authorize]
        public async Task<IActionResult> GetUserByName(string userName)
        {
            try
            {
                var user = await _account.GetByNameUserAsync(userName);

                if (user == null) return BadRequest();


                return Json(user);
            }
            catch (Exception ex)
            {
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }
        /// <summary>
        /// Создание роли
        /// </summary>
        /// <param name="role">Создаваемая роль</param>
        /// <returns>Результат создание роли. Возвращает <see cref="OkObjectResult"/> если роль создана или <see cref="BadRequestResult"/> при ошибке.</returns>
        [HttpPost("addrole")]
       // [Authorize]
        public async Task<IActionResult> SetRole(Role role)
        {
            try
            {
                var result = await _account.AddRoleAsync(role);

                if (!result) return BadRequest();


                return Ok("Роль создана");
            }
            catch (Exception ex)
            {
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }

        [HttpGet()]
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _account.GetUsersAsync();

                var userViewModels = new List<UserModel>();
                
                foreach (var user in users)
                {
                    // ИСПРАВЛЕНО: получаем роли КОНКРЕТНОГО пользователя, а не все роли системы
                    var userRoles = await _account.GetUserRolesAsync(user); // Используйте этот метод
                    
                    var userViewModel = new UserModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        SurName = user.SurName,
                        LastLogin = user.LastLogin,
                        EmailConfirmed = user.EmailConfirmed,
                        PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                        TwoFactorEnabled = user.TwoFactorEnabled,
                        LockoutEnabled = user.LockoutEnabled,
                        AccessFailedCount = user.AccessFailedCount,
                        Roles = [.. userRoles] // Используем реальные роли пользователя
                    };
                    
                    userViewModels.Add(userViewModel);
                }

                return View(userViewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка загрузки пользователей: {ex.Message}";
                return View(new System.Collections.Generic.List<UserModel>());
            }
        }

        /// <summary>
        /// Получение данных конкретного пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>Возвращает данные пользователя</returns>
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var user = await _account.GetByIdUserAsync(Guid.Parse(id));
                if (user == null)
                {
                    return NotFound();
                }
                
                var roles = await _account.GetUserRolesAsync(user);
                
                var viewModel = new UserModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    SurName = user.SurName,
                    LastLogin = user.LastLogin,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnabled = user.LockoutEnabled,
                    AccessFailedCount = user.AccessFailedCount,
                    Roles = roles
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка загрузки данных пользователя: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        
        [HttpPost("toggle-lock/{id}")]
        public async Task<IActionResult> ToggleLock(string id)
        {
            try
            {
                var user = await _account.GetByIdUserAsync(Guid.Parse(id));
                if (user == null)
                {
                    return NotFound();
                }
                
                user.LockoutEnabled = !user.LockoutEnabled;
                var result = await _account.UpdateUserAsync(user);
                
                if (result)
                {
                    TempData["Success"] = $"Статус блокировки пользователя {user.UserName} изменен";
                }
                else
                {
                    TempData["Error"] = $"Не удалось изменить статус блокировки";
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка изменения статуса блокировки: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("reset-password/{id}")]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            try
            {
                var user = await _account.GetByIdUserAsync(Guid.Parse(id));
                if (user == null)
                {
                    return NotFound();
                }
                
                var roles = await _account.GetUserRolesAsync(user);
                var token = _jwtGenerator.Seed(user.UserName, roles);
                var result = await _manager.ResetPasswordAsync(user, token, newPassword);
                
                if (result.Succeeded)
                {
                    TempData["Success"] = $"Пароль пользователя {user.UserName} успешно сброшен";
                }
                else
                {
                    TempData["Error"] = $"Не удалось сбросить пароль";
                }
                
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка сброса пароля: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }
        /// <summary>
        /// Обнавления пользователя
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
       [HttpPut("updateuser")]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateUserProfile(UpdateProfileRequest request)
        {
            try
            {
                var nameUser = User.Identity.Name;
                var user = await _account.GetByNameUserAsync(nameUser);
                if (user == null)
                {
                    return NotFound("Пользователь не найден");
                }

                bool updated = false;

                // Обновление личных данных
                if (!string.IsNullOrEmpty(request.FirstName) && request.FirstName != user.FirstName)
                {
                    user.FirstName = request.FirstName;
                    updated = true;
                }

                if (!string.IsNullOrEmpty(request.LastName) && request.LastName != user.LastName)
                {
                    user.LastName = request.LastName;
                    updated = true;
                }

                // Обновление телефона
                if (!string.IsNullOrEmpty(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
                {
                    user.PhoneNumber = request.PhoneNumber;
                    user.PhoneNumberConfirmed = false;
                    updated = true;
                }

                if (updated)
                {
                    var result = await _account.UpdateUserAsync(user);
                    if (!result)
                    {
                        // Возвращаем более информативное сообщение
                        return BadRequest(new 
                        { 
                            message = "Ошибка обновления профиля",
                            details = "Проверьте введенные данные"
                        });
                    }
                    
                    _logger.LogInformation("Профиль пользователя {UserId} обновлен", user.Id);
                }
                else
                {
                    _logger.LogDebug("Нет изменений для пользователя {UserId}", user.Id);
                }

                
                return Ok("success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка в UpdateUserProfile");
                return StatusCode(500, new 
                { 
                    message = "Внутренняя ошибка сервера",
                    error = ex.Message 
                });
            }
        }
    }
}
