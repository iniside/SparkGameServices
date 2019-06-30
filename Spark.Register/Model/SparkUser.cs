using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Spark.Register.Model
{
    public class SparkUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public IList<SparkUserExternal> ExternalId { get; set; }
    }
}
