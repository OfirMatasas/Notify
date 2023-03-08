using System.Threading.Tasks;
using Notify.Models;

namespace Notify.Services.Information
{
    public interface IInformationService
    {
        Task<DriverBasicInformationsModel> GetDriverInformation(string driver);
        Task<ConstructorBasicInformationsModel> GetTeamInformation(string team);
        Task<CircuitBasicInformationsModel> GetCircuitInformation(string country);
    }
}
