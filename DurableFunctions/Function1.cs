using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DurableFunctions.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DurableFunctions
{
    public static class Function1
    {
        [FunctionName("FanOutFanIn_Orchestrator")]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            WeatherRequest weatherRequest = context.GetInput<WeatherRequest>();

            // Fanning out
            var parallelActivities = new List<Task<string>>();
            foreach (var city in weatherRequest.Cities)
            {
                Task<string> task = context.CallActivityAsync<string>("Process_Request", city.Name);

                parallelActivities.Add(task);
            }

            await Task.WhenAll(parallelActivities);

            var sb = new StringBuilder();
            foreach (var completedParallelActivity in parallelActivities)
            {
                sb.AppendLine(completedParallelActivity.Result);
            }

            return sb.ToString();
        }

        [FunctionName("Process_Request")]
        public static async Task<string> GetWeatherAsync([ActivityTrigger] string name)
        {

            HttpClient client = new HttpClient();
            var uri = new StringBuilder();
            //uri.Append(Environment.GetEnvironmentVariable("WeatherApiUrlTemplate"));
            var response = await client.GetAsync((uri.ToString()));
            if (response.IsSuccessStatusCode)
            {
               var content = await response.Content.ReadAsAsync<WeatherResponse>();
                return $"Temperature in {name}, {content.location.country} is: {content.current.temp_c} C.";
            }
            return $"Could not retrieve info for: {name}!";
        }

        [FunctionName("FanOutFanIn_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var request = await req.Content.ReadAsAsync<WeatherRequest>();
            string instanceId = await starter.StartNewAsync("FanOutFanIn_Orchestrator", request);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            var response = await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId);

            return response;
        }
    }
}