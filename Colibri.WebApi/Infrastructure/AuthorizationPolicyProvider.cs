using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Colibri.WebApi.Infrastructure
{
    /// <summary>
    /// Поставщик политик авторизации.
    /// Наследует <see cref="DefaultAuthorizationPolicyProvider"/> и позволяет динамически создавать политики на основе разрешений.
    /// </summary>
    /// <remarks>
    /// Инициализирует класс <see cref="AuthorizationPolicyProvider"/>
    /// </remarks>
    /// <param name="options">Параметры авторизации.</param>
    public class AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
    {
        private readonly AuthorizationOptions _options = options.Value;
        /// <summary>
        /// Получает политику авторизации по имени. Если политика не существует, создает новую с требованием <see cref="PermissionRequirement"/>.
        /// </summary>
        /// <param name="policyName">Имя политики авторизации, которую необходимо получить.</param>
        /// <returns>Асинхронная задача, возвращающая политику авторизации.</returns>
        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // Проверяет, существует ли уже политика авторизации с заданным именем
            var policyExist = await base.GetPolicyAsync(policyName);

            if (policyExist != null)
            {
                return policyExist;
            }

            // Создаёт новую политику авторизации с требованием PermissionRequirement
            policyExist = new AuthorizationPolicyBuilder().AddRequirements(new PermissionRequirement(policyName)).Build();

            // Добавляет созданную политику в список политик
            _options.AddPolicy(policyName, policyExist);

            return policyExist;
        }
    }
    /// <summary>
    /// Требование авторизации для проверки разрешений.
    /// Реализует интерфейс <see cref="IAuthorizationRequirement"/>.
    /// </summary>
    /// <remarks>
    /// Инициализирует класс <see cref="PermissionRequirement"/>
    /// </remarks>
    /// <param name="policyName">Имя разрешения для проверки</param>
    public class PermissionRequirement(string policyName) : IAuthorizationRequirement
    {
        /// <summary>
        /// Имя разрешения для проверки
        /// </summary>
        public string PermissionName { get; set; } = policyName;
    }
}
