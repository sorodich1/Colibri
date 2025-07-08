using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;

namespace Colibri.WebApi.Helpers
{
    /// <summary>
    /// Помощник для работы с утверждениями (<see cref="Claim"/>).
    /// Предоставляет методы для создания утверждений и извлечения значений из них.
    /// </summary>
    public class ClaimHelper
    {
        /// <summary>
        /// Создает коллекцию утверждений на основе указанного объекта.
        /// </summary>
        /// <typeparam name="T">Тип объекта, на основе которого будут созданы утверждения.</typeparam>
        /// <param name="entity">Объект, из которого будут извлечены свойства для создания утверждений.</param>
        /// <param name="claims">Дополнительные утверждения, которые необходимо добавить к создаваемой коллекции.</param>
        /// <returns>Коллекция созданных утверждений (<see cref="IEnumerable{Claim}"/>).</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="entity"/> равен <c>null</c>.</exception>
        public static IEnumerable<Claim> CreateClaims<T>(T entity, IEnumerable<Claim> claims = null) where T : class
        {
            ArgumentNullException.ThrowIfNull(entity);

            List<Claim> list = [];

            if (claims != null)
            {
                list.AddRange(claims);
            }

            IEnumerable<Claim> collection = from t in typeof(T).GetProperties()
                                            where t.PropertyType.IsPrimitive || t.PropertyType.IsValueType || t.PropertyType
                                            == typeof(string)
                                            select t into property
                                            let value = property.GetValue(entity)
                                            where value != null
                                            select new Claim(property.Name, value.ToString());

            list.AddRange(collection);
            return list;
        }
        /// <summary>
        /// Извлекает значение утверждения по его имени из <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <typeparam name="T">Тип, в который будет преобразовано значение утверждения.</typeparam>
        /// <param name="identity">Идентичность, из которой будет извлечено значение.</param>
        /// <param name="claimName">Имя утверждения, значение которого необходимо получить.</param>
        /// <returns>Значение утверждения, преобразованное в указанный тип <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidOperationException">Возникает, если <paramref name="identity"/> равен <c>null</c>.</exception>
        public static T GetValue<T>(ClaimsIdentity identity, string claimName)
        {
            Claim claim = identity.FindFirst(x => x.Type == claimName);

            if (claim == null)
            {
                return default;
            }

            if (string.IsNullOrWhiteSpace(claim.Value))
            {
                return default;
            }

            try
            {
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(claim.Value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"{claim.Value} from {claim.Value} to {typeof(T)}", ex);
            }
        }
        /// <summary>
        /// Извлекает все значения утверждения по его имени из <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <typeparam name="T">Тип, в который будут преобразованы значения утверждений.</typeparam>
        /// <param name="items">Идентичности, из которой будут извлечены значения.</param>
        /// <param name="claimname">Имя утверждения, значения которого необходимо получить.</param>
        /// <returns>Список значений утверждений, преобразованных в указанный тип <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidOperationException">Возникает, если <paramref name="items"/> равен <c>null</c>.</exception>
        public static List<T> GetValues<T>(ClaimsIdentity items, string claimname)
        {
            var list = new List<T>();

            List<Claim> source = [.. items.FindAll((Claim x) => x.Type == claimname)];

            if (source.Count == 0)
            {
                return list;
            }

            source.ToList().ForEach(delegate (Claim x)
            {
                if (string.IsNullOrEmpty(x.Value))
                {
                    return;
                }
                try
                {
                    T item = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(x.Value);
                    list.Add(item);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"{x.Value} from {x.Value} to {typeof(T)}", ex);
                }
            });
            return list;
        }
    }
}
