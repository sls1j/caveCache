using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveCache.Nouns.Database
{
  public class Group
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> GroupMembers { get; set; }
    public List<string> Caves { get; set; }
  }
}
