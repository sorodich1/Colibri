using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Colibri.WebApi.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public static void UseRouteSlugify(this MvcOptions options)
        {
            options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        /// <summary>
        ///  Преобразует параметр маршрута в слаговый формат.
        /// </summary>
        /// <param name="value">Значение параметра, которое будет преобразовано.</param>
        /// <returns>Преобразованное значение параметра в формате слага, или null, если значение было null.</returns>
        public string TransformOutbound(object value)
        {
            // Преобразование значения в слаговый формат: добавление дефиса перед заглавными буквами и перевод в нижний регистр
            return value == null ? null : Regex.Replace(value.ToString(), "([a-z])([A-Z])", "$1-$2").ToLower();
        }
    }
}
