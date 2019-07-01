using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Spark.Authentication.Models
{
    public class SparkUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }
        public string ExternalId { get; set; }
        public string Provider { get; set; }
    }
}