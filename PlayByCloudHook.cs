using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SuperHobbyFriends.Civ6Notifier
{
    public static class PlayByCloudHook
    {
        static readonly string SlackWebhookUri = Environment.GetEnvironmentVariable("SlackWebhookUri");

        static HttpClient HttpClient = new HttpClient();

        [FunctionName("PlayByCloud")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("PlayByCloudHook triggered.");

            if (req.ContentType != "application/json")
            {
                log.LogError("Invalid request content-type.");
                return new BadRequestResult();
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Civ6Notification>(requestBody);

            log.LogInformation("Civ 6 webhook request body: {requestBody}.", requestBody);

            var res = await HttpClient.PostAsJsonAsync(
                SlackWebhookUri,
                new { text = $"It is {data.Player}'s turn {data.Turn} in {data.Game}." });

            return res.IsSuccessStatusCode
                ? new OkResult() as ActionResult
                : new BadRequestResult();
        }

        private class Civ6Notification
        {
            [JsonProperty("value1")]
            public string Game { get; set; }

            [JsonProperty("value2")]
            public string Player { get; set; }

            [JsonProperty("value3")]
            public int Turn { get; set; }
        }
    }
}
