using System.Threading.Tasks;

namespace ShelterCatOfTheDayFunction.LinksProviders
{
    public interface IDetailLinksProvider
    {
        Task<string[]> GetDetailsLinks();
    }
}