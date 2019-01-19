using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace caveCache.MongoDb
{
  public class UserSession
  {
    [BsonId]
    public ObjectId Id { get; set; }
    public ObjectId UserId { get; set; }
    public string SessionId { get; set; }
    public DateTime Timeout { get; set; }
    public bool IsCommandLine { get; set; }
    [BsonIgnore]
    public User User { get; set; }
  }
}
