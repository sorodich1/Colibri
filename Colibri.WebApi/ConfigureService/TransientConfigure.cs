using Colibri.ConnectNetwork.Services;
using Colibri.ConnectNetwork.Services.Abstract;
using Colibri.Data.Context;
using Colibri.Data.Services;
using Colibri.Data.Services.Abstracts;
using Colibri.WebApi.Services;
using Colibri.WebApi.Services.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Colibri.WebApi.ConfigureService
{
    /// <summary>
    /// 
    /// </summary>
    public class TransientConfigure
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void Configuration(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<ILoggerService, LoggerService>();
            services.AddTransient<IAppDbContext, AppDbContext>();
            services.AddTransient<IClientOrderService, ClientOrderService>();
            services.AddTransient<IHttpConnectService, HttpConnectService>();
            services.AddTransient<ITcpConnectService, TcpConnectService>();
            services.AddTransient<IFlightService, FlightService>();

            services.AddTransient<IJwtGenerator>(provider =>
            new JwtGenerator(
                configuration["Jwt:Key"],
                configuration["Jwt:Issuer"],
                configuration["Jwt:Audience"],
                configuration["Jwt:Secret"]
            ));
        }
    }
}
