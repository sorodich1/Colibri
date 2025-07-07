using Colibri.Data.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Colibri.Data.Infrastructure
{
    /// <summary>
    /// Класс для создания <see cref="ClaimsPrincipal"/> на основе пользователя.
    /// Наследуется от <see cref="UserClaimsPrincipalFactory{TUser, TRole}"/>.
    /// </summary>
    public class ApplicationClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, Role>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ApplicationClaimsPrincipalFactory"/>.
        /// </summary>
        /// <param name="userManager">Служба управления пользователями.</param>
        /// <param name="roleManager">Служба управления ролями.</param>
        /// <param name="options">Параметры идентификации.</param>
        public ApplicationClaimsPrincipalFactory(UserManager<User> userManager, RoleManager<Role> roleManager, IOptions<IdentityOptions> options) : base(userManager, roleManager, options)
        {
        }
        /// <summary>
        /// Создает <see cref="ClaimsPrincipal"/> для указанного пользователя.
        /// </summary>
        /// <param name="user">Пользователь, для которого создается <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>Задача, представляющая асинхронную операцию, содержащую созданный <see cref="ClaimsPrincipal"/>.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="user"/> равен <c>null</c>.</exception>
        public async override Task<ClaimsPrincipal> CreateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var principal = await base.CreateAsync(user);

            if (!string.IsNullOrEmpty(user.FirstName))
            {
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
            }
            if (!string.IsNullOrEmpty(user.LastName))
            {
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(ClaimTypes.Surname, user.LastName));
            }

            return principal;
        }
    }
}
