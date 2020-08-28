using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveCache.Nouns.Database
{
  public class UserSession
  {
    public string Id { get; set; }
    public string UserId { get; set; }
    public string SessionId { get; set; }
    public DateTime Timeout { get; set; }
    public bool IsCommandLine { get; set; }
    [BsonIgnore]
    public User User { get; set; }
  }
}
