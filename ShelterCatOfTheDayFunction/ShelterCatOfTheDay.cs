using AngleSharp;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Environment;

namespace ShelterCatOfTheDayFunction
{
    public static class ShelterCatOfTheDay
    {
        public static string DailyCatChannelWebhookUrl = string.Empty;

        private static readonly string PoznanShelterCatsWebsite = "http://schronisko.com/adopcje/koty/";

        [FunctionName("ShelterCatOfTheDay")]
        public static async Task Run([TimerTrigger("0 0 12 * * 1-5")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            DailyCatChannelWebhookUrl = GetEnvironmentVariable(nameof(DailyCatChannelWebhookUrl));

            var config = Configuration.Default.WithDefaultLoader();
            var browsingContext = BrowsingContext.New(config);

            var catPortfolioDetailsLinks = await GetCatPortfolioLinks(browsingContext)
                .ConfigureAwait(false);

            log.Info($"{catPortfolioDetailsLinks.Length} cat portfolio links downloaded");

            var chosenCatPortfolioLink = SelectCat(catPortfolioDetailsLinks);

            log.Info($"Selected cat: {chosenCatPortfolioLink}");

            var catPortfolio = await CreateCatPortfolio(browsingContext, chosenCatPortfolioLink)
                .ConfigureAwait(false);

            log.Info($"{catPortfolio.Name} cat portfolio created from link");

            var slackResponse = await SendSlackMessage(catPortfolio)
                .ConfigureAwait(false);

            if (slackResponse.IsSuccessStatusCode)
            {
                log.Info("Slack message send successfully!");
            }
            else
            {
                log.Error($"Slack message sending ended with error {slackResponse.StatusCode} {slackResponse.Content}");
            }
        }

        private static Task<HttpResponseMessage> SendSlackMessage(CatPortfolio catPortfolio)
        {
            var httpClient = new HttpClient();

            var catPortfolioSlackMessage = new
            {
                attachments = new[]
                {
                    new
                    {
                        color= "#36a64f",
                        title = $"Today's shelter cat of the day is {catPortfolio.Name}!",
                        title_link = catPortfolio.ProfileLink,
                        text = catPortfolio.Description,
                        image_url = catPortfolio.ImageLink
                    }
                }
            };

            var serializedMessage = JsonConvert.SerializeObject(catPortfolioSlackMessage);

            return httpClient.PostAsync(DailyCatChannelWebhookUrl, new StringContent(serializedMessage));
        }

        private static async Task<string[]> GetCatPortfolioLinks(IBrowsingContext browsingContext)
        {
            var document = await browsingContext
                .OpenAsync(PoznanShelterCatsWebsite)
                .ConfigureAwait(false);

            var catsPortfolioItemsSelector = "div.portfolio-wrapper div.portfolio-item";
            var portfolioItems = document.QuerySelectorAll(catsPortfolioItemsSelector);

            var catPortfolioDetailsLinks = portfolioItems
                .Select(m => m.QuerySelector("div.image-extras div.image-extras-content a.icon").GetAttribute("href"))
                .ToArray();

            return catPortfolioDetailsLinks;
        }

        private static string SelectCat(string[] catPortfolioDetailsLinks)
        {
            var random = new Random();
            var randomIndex = random.Next(0, catPortfolioDetailsLinks.Length - 1);

            return catPortfolioDetailsLinks[randomIndex];
        }

        private static async Task<CatPortfolio> CreateCatPortfolio(IBrowsingContext browsingContext, string catPortfolioLink)
        {
            var catPortfolioDocument = await browsingContext
                .OpenAsync(catPortfolioLink)
                .ConfigureAwait(false);

            var catPortfolio = new CatPortfolio
            {
                Name = catPortfolioDocument
                    .QuerySelector("div.project-content div.project-description > h2:nth-child(2)").TextContent,
                Description = catPortfolioDocument
                     .QuerySelector("div.project-content div.project-description > p:nth-child(3)").TextContent,
                ProfileLink = catPortfolioLink,
                ImageLink = catPortfolioDocument
                      .QuerySelector(".slides > li:nth-child(1) > a:nth-child(1) > img:nth-child(1)").GetAttribute("src")
            };

            return catPortfolio;
        }

        public class CatPortfolio
        {
            public string ImageLink { get; set; }

            public string ProfileLink { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }
        }
    }
}