using AngleSharp;
using ShelterCatOfTheDayFunction.Models;
using System;
using System.Threading.Tasks;

namespace ShelterCatOfTheDayFunction.SiteParsers
{
    public class AnimalPortfolioDetailsSiteParser : ISiteParser<Portfolio>
    {
        private readonly IBrowsingContext _browsingContext;

        public AnimalPortfolioDetailsSiteParser(IBrowsingContext browsingContext)
        {
            _browsingContext = browsingContext ?? throw new ArgumentNullException(nameof(browsingContext));
        }

        public async Task<Portfolio> ParseSiteToDataObject(string url)
        {
            var catPortfolioDocument = await _browsingContext
                .OpenAsync(url)
                .ConfigureAwait(false);

            var catPortfolio = new Portfolio
            {
                Name = catPortfolioDocument
                    .QuerySelector("div.wpb_wrapper h2").TextContent,
                Description = catPortfolioDocument
                    .QuerySelector("div.wpb_wrapper p").TextContent,
                ProfileLink = url,
                ImageLink = catPortfolioDocument
                    .QuerySelector("main div.wpb_wrapper div.w-image div.w-image-h  a").GetAttribute("href")
            };

            return catPortfolio;
        }
    }
}