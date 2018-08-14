using System;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;

namespace ShelterCatOfTheDayFunction.LinksProviders
{
    public class DogPortfolioDetailsLinkProvider : IDetailLinksProvider
    {
        private readonly IBrowsingContext _browsingContext;
        private readonly Url _websiteUrl;

        public DogPortfolioDetailsLinkProvider(IBrowsingContext browsingContext, Url websiteUrl)
        {
            _browsingContext = browsingContext ?? throw new ArgumentNullException(nameof(browsingContext));
            _websiteUrl = websiteUrl ?? throw new ArgumentNullException(nameof(websiteUrl));
        }

        public async Task<string[]> GetDetailsLinks()
        {
            var document = await _browsingContext
               .OpenAsync(_websiteUrl)
               .ConfigureAwait(false);

            var portfolioItemsSelector = "div.w-grid-list article.w-grid-item div.w-grid-item-h a.w-grid-item-anchor";
            var portfolioItems = document.QuerySelectorAll(portfolioItemsSelector);

            var portfolioDetailsLinks = portfolioItems
               .Select(m => m.GetAttribute("href"))
               .ToArray();

            return portfolioDetailsLinks;
        }
    }
}