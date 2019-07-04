using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Spark.SignalRClient
{
    class TokenClass
    {
        public String token { get; set; }
    }
    public class LoginWithProviderModel
    {
        public string Provider { get; set; }
        public string ExternalId { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            HttpClient client = new HttpClient();
            
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            LoginWithProviderModel m = new LoginWithProviderModel
            {
                Provider = "Provider",
                ExternalId = "87as5drtifuykvjha"
            };
            
            string ms =  System.Text.Json.Serialization.JsonSerializer.ToString(m);
            
            pairs.Add(new KeyValuePair<string, string>("ProviderName", "Microsoft"));
            pairs.Add(new KeyValuePair<string, string>("ProviderKey", "c9cc8a70ad19c2bc"));
            
            HttpContent content = new StringContent(ms);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            
            var r = client.PostAsync("https://localhost:8001/api/values/LoginWithExternalProvider", content).Result;//, content).Result;
            
            string c = r.Content.ReadAsStringAsync().Result;
            //var dd = System.Text.Json.Serialization.JsonSerializer.Parse<TokenClass>(c);

            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7001/hub/messagehub", o =>
                {
                    o.AccessTokenProvider = () => Task.FromResult(c);
                })
                .Build();

            connection.StartAsync();

            while (connection.State != HubConnectionState.Connected)
            {
                Console.WriteLine("Connecting");
            }

            Console.WriteLine("Hello World!");
            
        }
    }
}
