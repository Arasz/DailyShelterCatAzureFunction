using System.Threading.Tasks;
using ShelterCatOfTheDayFunction.Models;

namespace ShelterCatOfTheDayFunction.SiteParsers
{
    public interface ISiteParser<out TDataObject>
    {
        Task<Portfolio> ParseSiteToDataObject(string url);
    }
}