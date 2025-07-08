using Colibri.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Colibri.WebApi.Infrastructure
{
    /// <summary>
    /// Обработчик разрешений для проверки требований на основе претензий пользователя.
    /// Наследует <see cref="AuthorizationHandler{TRequirement}"/> для обработки требований типа <see cref="PermissionRequirement"/>.
    /// </summary>
    public class MicroservicePermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        /// <summary>
        /// Обрабатывает требования авторизации на основе разрешений.
        /// </summary>
        /// <param name="context">Контекст авторизации, который содержит информацию о пользователе и требованиях.</param>
        /// <param name="requirement">Требование авторизации, которое нужно проверить.</param>
        /// <returns>Асинхронная задача, представляющая завершение обработки.</returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // Приведение идентичности пользователя к ClaimsIdentity для получения претензий
            var identity = (ClaimsIdentity)context.User.Identity!;

            // Получение значения претензии, соответствующей разрешению из требования
            var claim = ClaimHelper.GetValue<string>(identity, requirement.PermissionName);

            // Если претензия не найдена, завершаем обработку без успеха
            if (claim == null)
            {
                return Task.CompletedTask;
            }

            // Если претензия найдена, помечаем требование как успешно выполненное
            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
