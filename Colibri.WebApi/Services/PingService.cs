using System.Threading.Tasks;
using Colibri.WebApi.Services.Abstract;

namespace Colibri.WebApi.Services;

public class PingService : IPingService
{
    public async Task<bool> PingHostAsync(string ipAddress)
    {
        try
        {
            using var ping = new System.Net.NetworkInformation.Ping();
            var reply = await ping.SendPingAsync(ipAddress, 3000); // timeout 3 секунды
            return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }
}
