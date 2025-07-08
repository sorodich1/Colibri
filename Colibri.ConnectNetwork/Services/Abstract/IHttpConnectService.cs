using System.Threading.Tasks;

namespace Colibri.ConnectNetwork.Services.Abstract
{
    public interface IHttpConnectService
    {
        Task<string> GetAsync(string url);
        Task<string> PostAsync(string url, string data);
        Task<string> PutAsync(string url, string data);
        Task<string> DeleteAsync(string url);
    }
}
