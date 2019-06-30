using System;

namespace Spark.Register.Events
{
    public class UserRegisteredEvent
    {
        public Guid UserId { get; set; }
        public string ExternalId { get; set; }
    }
}