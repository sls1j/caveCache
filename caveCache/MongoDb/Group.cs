using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace caveCache.MongoDb
{
  public class Group
  {
    [BsonId]
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public List<ObjectId> GroupMembers { get; set; }
    public List<ObjectId> Caves { get; set; }
  }
}
