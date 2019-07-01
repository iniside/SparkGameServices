using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Spark.Authentication.Models;

namespace Spark.Authentication.Repository
{
    public interface IUserRepository
    {
        Task AddUser(Guid UserId, string ExternalId, string Provider);
        Task<string> LoginWithExternalProvider(string ExternalId, string Provider);
    }

    public class UserRepository : IUserRepository
    {
        private readonly IMongoDatabase _database;

        public UserRepository(IOptions<Settings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }
        public async Task AddUser(Guid UserId, string ExternalId, string Provider)
        {
            IMongoCollection<SparkUser> Col = _database.GetCollection<SparkUser>("sparkUser");

            await Col.InsertOneAsync(new SparkUser
            {
                UserId = UserId,
                ExternalId = ExternalId,
                Provider = Provider
            });
        }

        public async Task<string> LoginWithExternalProvider(string ExternalId, string Provider)
        {
            var filterID = Builders<SparkUser>.Filter.Where(e => e.ExternalId == ExternalId);
            var filterProvider = Builders<SparkUser>.Filter.Where(e => e.Provider == Provider);

            FilterDefinition<SparkUser> d = new ObjectFilterDefinition<SparkUser>(filterID);
            FilterDefinition<SparkUser> o = new ObjectFilterDefinition<SparkUser>(filterProvider);

            var filter = Builders<SparkUser>.Filter.And(new List<FilterDefinition<SparkUser>>
            {
                d,
                o
            });

            var col = _database.GetCollection<SparkUser>("sparkUser");

            var res = await col.FindAsync(filterID);

            var user = await res.FirstAsync();

            return await Task.FromResult(user.UserId.ToString());
        }
    }
}