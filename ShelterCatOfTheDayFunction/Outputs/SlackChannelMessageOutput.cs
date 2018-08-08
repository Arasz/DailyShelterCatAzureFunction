using Newtonsoft.Json;
using ShelterCatOfTheDayFunction.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShelterCatOfTheDayFunction.Outputs
{
    public class SlackChannelMessageOutput : IExternalOutput<HttpResponseMessage, Portfolio>
    {
        private readonly string _webhookUrl;

        public SlackChannelMessageOutput(string webhookUrl)
        {
            _webhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
        }

        public Task<HttpResponseMessage> SendToExternalOutput(Portfolio data)
        {
            var httpClient = new HttpClient();

            var catPortfolioSlackMessage = new
            {
                attachments = new[]
                {
                    new
                    {
                        color= "#36a64f",
                        title = $"Today's shelter cat of the day is {data.Name}!",
                        title_link = data.ProfileLink,
                        text = data.Description,
                        image_url = data.ImageLink
                    }
                }
            };

            var serializedMessage = JsonConvert.SerializeObject(catPortfolioSlackMessage);

            return httpClient.PostAsync(_webhookUrl, new StringContent(serializedMessage));
        }
    }
}