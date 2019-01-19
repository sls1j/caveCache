using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace caveCache.MongoDb
{
  public class Group
  {
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public List<ObjectId> GroupMembers { get; set; }
    public List<ObjectId> Caves { get; set; }
  }
}
