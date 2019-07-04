using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RawRabbit;
using RawRabbit.Configuration;
using RawRabbit.DependencyInjection.ServiceCollection;
using RawRabbit.Enrichers.MessageContext.Context;
using RawRabbit.Instantiation;
using Spark.Authentication.Repository;
using Spark.Register.Events;

namespace Spark.Authentication
{
    public interface IEventHandler
    {

    }
    public class EventHandler : IEventHandler
    {
        private readonly IBusClient _client;
        private readonly IUserRepository _userRepository;
        public EventHandler(IBusClient client, IUserRepository userRepository)
        {
            _client = client;
            _userRepository = userRepository;

            _client.SubscribeAsync<UserRegisteredEvent>(ServerValuesAsync);
        }

        private async Task ServerValuesAsync(UserRegisteredEvent message)
        {
            await _userRepository.AddUser(message.UserId, message.ExternalId, message.Provider);
            
        }
    }

    public class Settings
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
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
            services.Configure<Settings>(options =>
            {
                options.ConnectionString
                    = Configuration.GetSection("MongoConnection:ConnectionString").Value;
                options.Database
                    = Configuration.GetSection("MongoConnection:Database").Value;
            });

            services.AddCors();

            services.AddRawRabbit(new RawRabbitOptions
            {
                ClientConfiguration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("rawrabbit.json")
                    .Build()
                    .Get<RawRabbitConfiguration>(),
            });

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IEventHandler, EventHandler>();
            services.AddControllers(o =>
            {
                
            });
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
            var eventBus = app.ApplicationServices.GetRequiredService<IEventHandler>();
            app.UseCors();
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
