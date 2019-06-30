using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Spark.Register.Model
{
    public class SparkUserExternal
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }
        public string ExternalId { get; set; }
        public string ProviderName { get; set; }
    }
}