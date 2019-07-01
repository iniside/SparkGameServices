using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spark.Authentication.Controllers
{
    class TokenJson
    {
        public string Token { get; set; }
    }
    //[Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [Route("api/values/login")]
        [HttpPost]
        public string Login()//string Login, string Password)
        {
            var ru = new RequestUrl("https://localhost:6001/connect/authorize");
            var payload = new
            {
                token = "mmm"
            };
            TokenClientOptions tco = new TokenClientOptions();
            // create token client
            //var client = new TokenClient(tco);

            // send custom grant to token endpoint, return response
            //return await client.RequestCustomGrantAsync("delegation", "api2", payload);
            var client = new HttpClient();
            var s = new TokenJson
            {
                Token = "123"
            };
            var d = System.Text.Json.Serialization.JsonSerializer.ToString(s);
            HttpContent con = new StringContent(d);
            con.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var dupa = new Dictionary<string, string>();
            dupa.Add("Token", "123");
            var response = client.RequestTokenAsync(new TokenRequest
            {
                 Address = "https://localhost:6001/connect/token",

                 ClientId = "userid",
                 ClientSecret = "apisecret",
                 GrantType = "userid",
                 Parameters = dupa

            }).Result;

            
            //var response = client.RequestPasswordTokenAsync(new PasswordTokenRequest
            //{
            //    Address = "https://localhost:6001/connect/token",

            //    ClientId = "ApiClient",
            //    ClientSecret = "apisecret",
            //    Scope = "profile",

            //    UserName = "damienbod",
            //    Password = "damienbod"
            //}).Result;

            if (response.IsError)
            {

            }

            return response.AccessToken;
        }
    }
}
