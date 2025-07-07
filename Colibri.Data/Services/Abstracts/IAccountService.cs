using Colibri.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Colibri.Data.Services.Abstracts
{
    /// <summary>
    /// Интерфейс для управления учетными записями пользователей.
    /// Предоставляет методы для получения, добавления, обновления и удаления пользователей, а также для управления ролями.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Получает пользователя по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор пользователя.</param>
        /// <returns>Асинхронная задача, возвращающая объект пользователя с указанным идентификатором или <c>null</c>, если пользователь не найден.</returns>
        Task<User> GetByIdUserAsync(Guid id);
        /// <summary>
        /// Получает роль по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор роли.</param>
        /// <returns>Асинхронная задача, возвращающая объект роли с указанным идентификатором или <c>null</c>, если роль не найдена.</returns>
        Task<Role> GetByIdRoleAsync(Guid id);
        /// <summary>
        /// Получает пользователя по его имени.
        /// </summary>
        /// <param name="name">Имя пользователя</param>
        /// <returns>Асинхронная задача, возвращающая объект пользователя с именем или <c>null</c>, если пользователь не найден.</returns>
        Task<User> GetByNameUserAsync(string name);
        /// <summary>
        /// Получает список всех пользователей.
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список объектов пользователей.</returns>
        Task<List<User>> GetUsersAsync();
        /// <summary>
        /// Получает список всех ролей.
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список объектов ролей.</returns>
        Task<List<Role>> GetRolsAsync();
        /// <summary>
        /// Добавляет нового пользователя с ролью user.
        /// </summary>
        /// <param name="user">Объект пользователя, который должен быть добавлен.</param>
        /// <param name="password">Пароль для установки пользователя</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если пользователь успешно добавлен; в противном случае <c>false</c>.</returns>
        Task<bool> AddUserAsync(User user, string password);
        /// <summary>
        /// Добавляет новую роль.
        /// </summary>
        /// <param name="role">Объект роли, которая должна быть добавлена.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если роль успешно добавлена; в противном случае <c>false</c>.</returns>
        Task<bool> AddRoleAsync(Role role);
        /// <summary>
        /// Назначение роли пользователю
        /// </summary>
        /// <param name="user">Пользователь для которого назначается роль</param>
        /// <param name="role">Назначаемая роль пользователю</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если роль успешно добавлена для пользователя; в противном случае <c>false</c>.</returns>
        Task<bool> RoleAssignmentAsync(User user, string role);
        /// <summary>
        /// Удаление роли у пользователя
        /// </summary>
        /// <param name="user">Пользователь для которого удаляется роль</param>
        /// <param name="role">Удаляемая роль у пользователя</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если роль успешно удалена у пользователя; в противном случае <c>false</c>.</returns>
        Task<bool> RoleCancelAsync(User user, string role);
        /// <summary>
        /// Обновляет информацию о пользователе. Может также сбросить пароль.
        /// </summary>
        /// <param name="user">Объект пользователя, информацию о котором необходимо обновить.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если обновление прошло успешно; в противном случае <c>false</c>.</returns>
        Task<bool> UpdateUserAsync(User user);
        /// <summary>
        /// Обновляет информацию о роли.
        /// </summary>
        /// <param name="roleName">Имя роли, информацию о котором необходимо обновить.</param>
        /// <param name="roleId">Объект роли, информацию о котором необходимо обновить.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если обновление прошло успешно; в противном случае <c>false</c>.</returns>
        Task<bool> UpdateRoleAsync(Guid roleId, string roleName);
        /// <summary>
        /// Удаляет пользователя по указанному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор пользователя, которого нужно удалить.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если пользователь был успешно удален; в противном случае <c>false</c>.</returns>
        Task<bool> DeleteUserAsync(Guid id);
        /// <summary>
        /// Удаляет роль по указанному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор роли, которую нужно удалить.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если роль была успешно удалена; в противном случае <c>false</c>.</returns>
        Task<bool> DeleteRoleAsync(Guid id);
        /// <summary>
        /// Проверка пароля пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="password">Пароль</param>
        /// <returns></returns>
        Task<bool> CheckPasswordAsync(User user, string password);
        /// <summary>
        /// Роли конкретного пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns></returns>
        Task<List<string>> GetUserRolesAsync(User user);
    }
}
