using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RawRabbit;
using RawRabbit.Configuration;
using RawRabbit.Enrichers.GlobalExecutionId;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Enrichers.MessageContext.Context;
using RawRabbit.Instantiation;
using Spark.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spark.Events
{
    public class MessageWrapper
    {
        public string MessageType { get; set; }

        public string MessageBody { get; set; }
    }
    public class MessageWrapperResponse
    {
        public string UserId { get; set; }
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
namespace Spark.TestService
{
    class Program
    {
        private static IBusClient _client;

        public static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }

        public static async Task RunAsync()
        {
            //Log.Logger = new LoggerConfiguration()
            //    .WriteTo.LiterateConsole()
            //    .CreateLogger();

            _client = RawRabbitFactory.CreateSingleton(new RawRabbitOptions
            {
                ClientConfiguration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("rawrabbit.json")
                    .Build()
                    .Get<RawRabbitConfiguration>()
            });

            //Type t = Type.GetType("AskEvent");
            //var tt = typeof(AskEvent);
            //
            //object o = new AskEvent { Question = "Ask Question" };
            //
            //await _client.PublishAsync(o);
            await _client.SubscribeAsync<TestMessage>(ServerValuesAsync);
            //await _client.RespondAsync<ValueRequest, ValueResponse>(request => SendValuesThoughRpcAsync(request));
        }
        private static  async Task ServerValuesAsync(TestMessage message)
        {
            TestMessageResponse respond = new TestMessageResponse();
            respond.UserId = message.UserId;
            respond.SomeMessage = "responded to you!";
            var msgBody = JsonSerializer.ToString(respond);

            MessageWrapperResponse Msg = new MessageWrapperResponse
            {
                UserId = message.UserId,
                MessageType = typeof(TestMessageResponse).FullName,
                MessageBody = msgBody
            };

            //var respBody = JsonSerializer.ToString(Msg);

            await _client.PublishAsync(Msg);
        }
    }
}
