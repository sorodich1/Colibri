using Colibri.Data.Entity;
using System.Threading.Tasks;

namespace Colibri.Data.Services.Abstracts
{
    public interface IFlightService
    {
        Task AddFlightWaipoints(Waypoint waypoint);
    }
}
