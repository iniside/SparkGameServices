using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityServer4.AccessTokenValidation;
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

namespace Spark.Gateway
{
    //generic hub to route messages from backend to all clients.
    public class MessageHub : Hub
    {

    }
    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.Identity?.Name;
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddAuthentication(o =>
            //{
            //    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //
            //})

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
                //.AddIdentityServerAuthentication(options =>
                //{
                //    options.Authority = "http://localhost:6001";
                //    options.RequireHttpsMetadata = false;
                //    //options.ApiName = "my-api";
                //    options.NameClaimType = "sub";
                //    options.SaveToken = true;
                //    options.ApiSecret = "apisecret".Sha256();
                //    //options.TokenRetriever = new Func<HttpRequest, string>(req =>
                //    //{
                //    //    var fromHeader = TokenRetrieval.FromAuthorizationHeader();
                //    //    var fromQuery = TokenRetrieval.FromQueryString();
                //    //    return fromHeader(req) ?? fromQuery(req);
                //    //}
                //})
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MessageHub>("/hub/messagehub");
            });
        }
    }
}
