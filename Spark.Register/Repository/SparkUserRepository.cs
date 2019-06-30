using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Spark.Register.Model;

namespace Spark.Register.Repository
{
    public interface ISparkUserRepository
    {
        Task<Guid> RegisterUserWithPassword(SparkUser NewUser);
        Task<Guid> RegisterUserWithExternalId(SparkUser NewUser, string ExternalName, string ExternalId);
        Task<SparkUser> FindByExternalId(string ExternalId);
    }
    public class SparkUserRepository : ISparkUserRepository
    {
        private readonly IMongoDatabase _database;

        public SparkUserRepository(IOptions<Settings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }

        public async Task<Guid> RegisterUserWithPassword(SparkUser NewUser)
        {
            NewUser.UserId = Guid.NewGuid();
            NewUser.Id = NewUser.UserId;

            IMongoCollection<SparkUser> Col =  _database.GetCollection<SparkUser>("sparkRegisterdUser");

            await Col.InsertOneAsync(NewUser);

            return await Task.FromResult(NewUser.UserId);
        }

        public async Task<Guid> RegisterUserWithExternalId(SparkUser NewUser, string ExternalName, string ExternalId)
        {
            NewUser.UserId = Guid.NewGuid();
            NewUser.Id = NewUser.UserId;

            SparkUserExternal ex = new SparkUserExternal
            {
                Id = Guid.NewGuid(),
                UserId = NewUser.UserId,
                ExternalId = ExternalId,
                ProviderName = ExternalName
            };
            NewUser.ExternalId = new List<SparkUserExternal>();
            NewUser.ExternalId.Add(ex);

            IMongoCollection<SparkUser> Col = _database.GetCollection<SparkUser>("sparkRegisterdUser");
            
            await Col.InsertOneAsync(NewUser);

            return await Task.FromResult(NewUser.UserId);
        }

        public async Task<SparkUser> FindByExternalId(string ExternalId)
        {
            var filter = Builders<SparkUser>.Filter.ElemMatch(x => x.ExternalId, x => x.ExternalId == ExternalId);

            var col = _database.GetCollection<SparkUser>("sparkRegisterdUser");

            var res = await col.FindAsync(filter);

            var user = await res.FirstAsync();

            return await Task.FromResult(user);
        }
    }
}