using Colibri.Data.Entity;
using Colibri.Data.Services.Abstracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Colibri.Data.Services
{
    /// <summary>
    ///  Сервис для управления учетными записями пользователей.
    ///  Реализует <see cref="IAccountService"/>.
    /// </summary>
    /// <param name="roleManager">Служба управления ролями.</param>
    /// /// <param name="userManager">Служба управления пользователями.</param>
    public class AccountService(RoleManager<Role> roleManager, UserManager<User> userManager) : IAccountService
    {
        private readonly RoleManager<Role> _roleManager = roleManager;
        private readonly UserManager<User> _userManager = userManager;
        /// <summary>
        /// Добавляет новоую роль
        /// </summary>
        /// <param name="role">Добавляемая роль</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если роль добавлена успешно; в противном случае <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибки добавления</exception>
        public async Task<bool> AddRoleAsync(Role role)
        {
            try
            {
                if (role == null)
                    return false;

                var existingRole = await _roleManager.FindByNameAsync(role.Name);

                if (existingRole == null)
                {
                    var result = await _roleManager.CreateAsync(role);
                    return result.Succeeded;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка добавления роли пользователя", ex);
            }
        }
        /// <summary>
        /// Добавление пользователя в базу данных
        /// </summary>
        /// <param name="user">Сущность пользователя</param>
        /// <param name="password">Устанавлевыемый пароль для пользователя</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если пользователь добавлен успешно; в противном случае <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибки добавления</exception>
        public async Task<bool> AddUserAsync(User user, string password)
        {
            try
            {
                if (user == null || string.IsNullOrEmpty(password))
                    return false;

                var existingUser = await _userManager.FindByNameAsync(user.UserName);

                if (existingUser == null)
                {
                    var result = await _userManager.CreateAsync(user, password);
                    return result.Succeeded;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка добавления пользователя", ex);
            }
        }
        /// <summary>
        /// Проверяет, совпадает ли указанный пароль с паролем пользователя.
        /// </summary>
        /// <param name="user">Пользователь, пароль которого будет проверяться.</param>
        /// <param name="password">Пароль для проверки.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если пароль верный; в противном случае <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            try
            {
                return await _userManager.CheckPasswordAsync(user, password);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка проверки пароля пользователя", ex);
            }
        }
        /// <summary>
        /// Удаление роли из базы данных
        /// </summary>
        /// <param name="id">Идентификатор роли</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если роль успешно удалена; в противном случае <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибки удаления</exception>
        public async Task<bool> DeleteRoleAsync(Guid id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id.ToString());

                if (role == null) return false;

                var result = await _roleManager.DeleteAsync(role);

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка удаления роли пользователя", ex);
            }
        }
        /// <summary>
        /// Удаление пользователя с базы данных
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если пользователь удалён успешно; в противном случае <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибки удаления</exception>
        public async Task<bool> DeleteUserAsync(Guid id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());

                if (user == null)
                    return false;

                var result = await _userManager.DeleteAsync(user);

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка удаления пользователя", ex);
            }
        }
        /// <summary>
        /// Получение роли по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор роли</param>
        /// <returns>Асинхронная задача, возвращающая роль пользователя</returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибки выборки</exception>
        public async Task<Role> GetByIdRoleAsync(Guid id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null) return null;
                return role;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки роли пользователя по идентификатору", ex);
            }
        }
        /// <summary>
        /// Выборка пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>Асинхронная задача, возвращающая пользователя</returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибки выборки</exception>
        public async Task<User> GetByIdUserAsync(Guid id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null) return null;
                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки пользователя по идентификатору", ex);
            }
        }
        /// <summary>
        /// Получение пользователя по его логину
        /// </summary>
        /// <param name="name">Имя пользователя</param>
        /// <returns>Асинхронная задача, возвращающая пользователя</returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибки выборки</exception>
        public async Task<User> GetByNameUserAsync(string name)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(name);
                if (user == null) return null;
                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки пользователя по имени", ex);
            }
        }
        /// <summary>
        /// Выборка всех ролей пользователя
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список ролей пользователя</returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибки выборки</exception>
        public async Task<List<Role>> GetRolsAsync()
        {
            try
            {
                var roles = _roleManager.Roles;
                return await roles.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки списка всех ролей пользователя", ex);
            }
        }
        /// <summary>
        /// Получает роли, назначенные указанному пользователю.
        /// </summary>
        /// <param name="user">Пользователь для которого нужно получить роли.</param>
        /// <returns>Асинхронная задача, возвращающая список ролей, назначенных пользователю.</returns>
        /// <exception cref="InvalidOperationException">Исключение при выборки роли пользователя</exception>
        public async Task<List<string>> GetUserRolesAsync(User user)
        {
            try
            {
                return [.. await _userManager.GetRolesAsync(user)];
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки ролей пользователя", ex);
            }
        }
        /// <summary>
        /// Выборка всех пользователей из бд
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список всех пользователей</returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибки выборки</exception>
        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                return await _userManager.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка выборки списка всех пользователей", ex);
            }
        }
        /// <summary>
        /// Назначение роли пользователю
        /// </summary>
        /// <param name="user">Пользователь которому назначается роль</param>
        /// <param name="role">Назначаемая роль пользователю</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если роль успешно назначена пользователю; в противном случае <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибки назначения</exception>
        public async Task<bool> RoleAssignmentAsync(User user, string role)
        {
            try
            {
                var roleExist = await _roleManager.RoleExistsAsync(role);

                if (roleExist)
                {
                    var addToRoleResult = await _userManager.AddToRoleAsync(user, role);
                    return addToRoleResult.Succeeded;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка назначения роли пользователя", ex);
            }
        }
        /// <summary>
        /// Отменить роль у пользователя. 
        /// </summary>
        /// <param name="user">Пользователь у которого удаляется роль</param>
        /// <param name="role">Отменяемая роль пользователя</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если роль успешно отменена; в противном случае <c>false</c></returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибки отмены</exception>
        public async Task<bool> RoleCancelAsync(User user, string role)
        {
            try
            {
                var result = await _userManager.RemoveFromRoleAsync(user, role);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка удаления роли у пользователя", ex);
            }
        }
        /// <summary>
        /// Обнавление роли
        /// </summary>
        /// <param name="roleId">Идентификатор роли</param>
        /// <param name="roleName">Новое имя роли</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если роль успешно обновлена; в противном случае <c>false</c></returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибки обнавления</exception>
        public async Task<bool> UpdateRoleAsync(Guid roleId, string roleName)
        {
            try
            {
                var existingRole = await _roleManager.FindByIdAsync(id.ToString());
                if (existingRole == null) return false;

                existingRole.Name = roleName;
                existingRole.NormalizedName = roleName.ToUpper();

                var result = await _roleManager.UpdateAsync(existingRole);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка обнавления роли пользователя", ex);
            }
        }
        /// <summary>
        /// Обнавление пользователя
        /// </summary>
        /// <param name="user">Пользователь обнавляемый</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если пользователя успешно обновлен; в противном случае <c>false</c></returns>
        /// <exception cref="InvalidOperationException">Исключение при ошибки обнавления</exception>
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка обнавления пользователя", ex);
            }
        }
    }
}
