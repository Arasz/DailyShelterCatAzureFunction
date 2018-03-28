using AngleSharp;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShelterCatOfTheDayFunction
{
    public static class ShelterCatOfTheDay
    {
        [FunctionName("ShelterCatOfTheDay")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            var config = Configuration.Default.WithDefaultLoader();
            var browsingContext = BrowsingContext.New(config);
            var poznanShelterCatsWebsite = "http://schronisko.com/adopcje/koty/";
            var document = await browsingContext
                .OpenAsync(poznanShelterCatsWebsite);

            var catsPortfolioItemsSelector = "div.portfolio-wrapper div.portfolio-item";
            var portfolioItems = document.QuerySelectorAll(catsPortfolioItemsSelector);

            var catPortfolios = portfolioItems.Select(m => new CatPortfolio
            {
                ImageLink = m.QuerySelector("div.image a").GetAttribute("href"),
                ProfileLink = m.QuerySelector("div.image-extras div.image-extras-content a.icon").GetAttribute("href")
            }).ToList();

            foreach (var catPortfolio in catPortfolios.Take(1))
            {
                var catPortfolioDocument = await browsingContext.OpenAsync(catPortfolio.ProfileLink);

                catPortfolio.Name = catPortfolioDocument.QuerySelector("div.project-content div.project-description > h2:nth-child(2)").TextContent;
                catPortfolio.Description = catPortfolioDocument.QuerySelector("div.project-content div.project-description > p:nth-child(3)").TextContent;

                log.Info($"Cat portfolio:{Environment.NewLine}{catPortfolio}{Environment.NewLine}");
            }
        }

        public class CatPortfolio
        {
            public string ImageLink { get; set; }

            public string ProfileLink { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public override string ToString()
            {
                return $"{nameof(ImageLink)}: {ImageLink}, {nameof(ProfileLink)}: {ProfileLink}, {nameof(Name)}: {Name}, {nameof(Description)}: {Description}";
            }
        }
    }
}