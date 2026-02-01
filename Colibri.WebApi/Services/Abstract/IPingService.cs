using System.Threading.Tasks;

namespace Colibri.WebApi.Services.Abstract;

public interface IPingService
{
    Task<bool> PingHostAsync(string ipAddress);
}
