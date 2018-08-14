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
        private static string DailyAnimalChannelWebhookUrl = string.Empty;

        private static readonly string PoznanShelterAnimalsWebsite = "http://schronisko.com/zwierzeta";

        [FunctionName("ShelterCatOfTheDay")]
        public static async Task Run([TimerTrigger("0 0 12 * * 1-5")] TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            if (string.IsNullOrEmpty(DailyAnimalChannelWebhookUrl))
            {
                DailyAnimalChannelWebhookUrl = GetEnvironmentVariable(nameof(DailyAnimalChannelWebhookUrl));   
            }
            
            var config = Configuration.Default.WithDefaultLoader();
            var browsingContext = BrowsingContext.New(config);
            var randomChooser = new RandomLinkChooser();
            var portfolioDetailsSiteParser = new AnimalPortfolioDetailsSiteParser(browsingContext);
            var slackIntegration = new SlackChannelMessageOutput(DailyAnimalChannelWebhookUrl);

            
            var catPortfolioDetailsLinksProvider = new PortfolioDetailsLinkProvider(
                browsingContext,
                Url.Create($"{PoznanShelterAnimalsWebsite}/koty"));
            
            var dogPortfoliDetailsLinksProvider = new PortfolioDetailsLinkProvider(
                browsingContext, 
                Url.Create($"{PoznanShelterAnimalsWebsite}/psy"));
            
            var linksProviders = new IDetailLinksProvider[]{catPortfolioDetailsLinksProvider, dogPortfoliDetailsLinksProvider };

            foreach (var detailLinksProvider in linksProviders)
            {
                var portfolioDetailsLinks = await detailLinksProvider.GetDetailsLinks();

                log.Info($"{portfolioDetailsLinks.Length} portfolio links downloaded");

                var chosenPortfolioLink = randomChooser.Choose(portfolioDetailsLinks);

                log.Info($"Selected animal: {chosenPortfolioLink}");

                var portfolio = await portfolioDetailsSiteParser.ParseSiteToDataObject(chosenPortfolioLink);

                log.Info($"{portfolio.Name} portfolio created from link");

                var slackResponse = await slackIntegration.SendToExternalOutput(portfolio);

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
}