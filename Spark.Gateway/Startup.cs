using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;
using RawRabbit;
using RawRabbit.Configuration;
using RawRabbit.DependencyInjection.ServiceCollection;
using RawRabbit.Instantiation;
using Spark.Events;

namespace Spark.Events
{
    public class MessageWrapper
    {
        public string MessageType { get; set; }

        public string MessageBody { get; set; }
    }

    public class TestMessage
    {
        public string UserId { get; set; }
        public string SomeMessage { get; set; }
    }

    public class TestMessageResponse
    {
        public string UserId { get; set; }
        public string SomeMessage { get; set; }
    }
}

namespace Spark.Gateway
{
    public class MessageWrapper
    {
        public string MessageType { get; set; }

        public string MessageBody { get; set; }
    }
    //generic hub to route messages from backend to all clients.
    public class MessageHub : Hub
    {
        private readonly IBusClient _client;

        public MessageHub(IBusClient client)
        {
            _client = client;
        }

        public async Task HandleMessage(string message)
        {
            if (message != null)
            {
                if (message.Length > 0)
                {
                    MessageWrapper msg = System.Text.Json.Serialization.JsonSerializer.Parse<MessageWrapper>(message);

                    object testMsg = System.Text.Json.Serialization.JsonSerializer.Parse(
                                        msg.MessageBody, Type.GetType(msg.MessageType));

                    await _client.PublishAsync(testMsg);

                    //await Clients.Users("fcc4482d-9f24-4968-9f7e-93f79f00cee6")
                    //    .SendCoreAsync("MMessageResponse", new object[1]
                    //    {
                    //        "123"
                    //    });
                }
            }
        }
    }
    public interface IEventHandler
    {

    }
    public class EventHandler : IEventHandler
    {
        private readonly IBusClient _client;
        private readonly IHubContext<MessageHub> _hubContext;
        public EventHandler(IBusClient client, IHubContext<MessageHub> hubContext)
        {
            _client = client;
            _hubContext = hubContext;

            _client.SubscribeAsync<Spark.Events.TestMessageResponse>(ServerValuesAsync);
        }

        private async Task ServerValuesAsync(Events.TestMessageResponse arg)
        {
            await _hubContext.Clients.Users(arg.UserId).SendCoreAsync("MessageHandler", new object[1]
            {
                arg.SomeMessage
            });
        }
    }

    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            string id = connection?.User?.FindFirst("id").Value;
            return id;
        }
    }

    public class Startup
    {
        private IEventHandler _handler;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = "http://localhost:6001",
                ValidAudience = "dataEventRecords",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("apisecret")),
                NameClaimType = "name",
                RoleClaimType = "role",
            };

            services
                .AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.Authority = "https://localhost:6001";
                    o.SaveToken = true;
                    o.TokenValidationParameters = tokenValidationParameters;
                    // We have to hook the OnMessageReceived event in order to
                    // allow the JWT authentication handler to read the access
                    // token from the query string when a WebSocket or 
                    // Server-Sent Events request comes in.
                    o.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var asd = context.Request.Headers.Values;

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/hub/messagehub")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            services.AddSignalR();
            services.AddRawRabbit(new RawRabbitOptions
            {
                ClientConfiguration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("rawrabbit.json")
                    .Build()
                    .Get<RawRabbitConfiguration>(),
            });
            services.AddTransient<IEventHandler, EventHandler>();
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            _handler = app.ApplicationServices.GetRequiredService<IEventHandler>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MessageHub>("/hub/messagehub");
            });
        }
    }
}
