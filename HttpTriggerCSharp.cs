using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;


namespace Company.Function
{
    public static class HTTPTriggerGetRoute
    {
        [FunctionName("HTTPTriggerGetRoute")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Route")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            
            string appId = req.Query["appId"];
            string appCode = req.Query["appCode"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            
            appId = appId ?? data?.appId;
            appCode = appCode ?? data?.appCode;

            ProcessRepositories(appId,appCode).Wait();

            //return name != null
            //    ? (ActionResult)new OkObjectResult($"Hello, {name}")
            //    : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
            return (ActionResult)new OkObjectResult($"Hello, {appId}");
        }

        private static readonly HttpClient client = new HttpClient();
        private static async Task ProcessRepositories(string appId, string appCode)
        {
            var serializer = new DataContractJsonSerializer(typeof(List<routes>));
            client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(
            //new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            //client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var stringTask = client.GetStringAsync($"https://route.api.here.com/routing/7.2/calculateroute.json?app_id={appId}&app_code={appCode}&waypoint0=geo!52.5,13.4&waypoint1=geo!52.5,13.45&mode=fastest;car;traffic:disabled");
            var streamTask = client.GetStreamAsync($"https://route.api.here.com/routing/7.2/calculateroute.json?app_id={appId}&app_code={appCode}&waypoint0=geo!52.5,13.4&waypoint1=geo!52.5,13.45&mode=fastest;car;traffic:disabled");
            var routes = serializer.ReadObject(await streamTask) as List<routes>;
            var msg = await stringTask;
            Console.Write(msg);
            foreach (var route in routes)
            Console.WriteLine(route.start);
    
        }
    }

    public class routes
    {
        public string start;
    }

   
}

