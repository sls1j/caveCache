using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaveCache.Nouns.Database
{
  public class Global
  {
    public string Id { get; set; }
    public bool IsBootStrapped { get; set; }
    public int CaveNumber { get; set; }
  }
}
