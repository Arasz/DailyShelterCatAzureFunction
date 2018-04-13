using AngleSharp;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using ShelterCatOfTheDayFunction.LinkChoosers;
using ShelterCatOfTheDayFunction.LinksProviders;
using ShelterCatOfTheDayFunction.Outputs;
using ShelterCatOfTheDayFunction.SiteParsers;
using System;
using System.Threading.Tasks;
using static System.Environment;

namespace ShelterCatOfTheDayFunction
{
    public static class ShelterCatOfTheDay
    {
        public static string DailyCatChannelWebhookUrl = string.Empty;

        private static readonly string PoznanShelterCatsWebsite = "http://schronisko.com/zwierzeta/koty";

        [FunctionName("ShelterCatOfTheDay")]
        public static async Task Run([TimerTrigger("0 */1 * * * 1-5")] TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            DailyCatChannelWebhookUrl = GetEnvironmentVariable(nameof(DailyCatChannelWebhookUrl));

            var config = Configuration.Default.WithDefaultLoader();
            var browsingContext = BrowsingContext.New(config);

            var catPortfolioDetailsLinksProvider =
                new CatPortfolioDetailsLinksProvider(browsingContext, Url.Create(PoznanShelterCatsWebsite));
            var randomChooser = new RandomLinkChooser();
            var catPortfolioDetailsSiteParser = new CatPortfolioDetailsSiteParser(browsingContext);
            var slackIntegration = new SlackChannelMessageOutput(DailyCatChannelWebhookUrl);

            var catPortfolioDetailsLinks = await catPortfolioDetailsLinksProvider.GetDetailsLinks()
                .ConfigureAwait(false);

            log.Info($"{catPortfolioDetailsLinks.Length} cat portfolio links downloaded");

            var chosenCatPortfolioLink = randomChooser.Choose(catPortfolioDetailsLinks);

            log.Info($"Selected cat: {chosenCatPortfolioLink}");

            var catPortfolio = await catPortfolioDetailsSiteParser.ParseSiteToDataObject(chosenCatPortfolioLink)
                .ConfigureAwait(false);

            log.Info($"{catPortfolio.Name} cat portfolio created from link");

            var slackResponse = await slackIntegration.SendToExternalOutput(catPortfolio)
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
    }
}