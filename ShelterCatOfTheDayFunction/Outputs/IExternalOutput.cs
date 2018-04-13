using System.Threading.Tasks;

namespace ShelterCatOfTheDayFunction.Outputs
{
    public interface IExternalOutput<TResponse, in TDataObject>
    {
        Task<TResponse> SendToExternalOutput(TDataObject data);
    }
}