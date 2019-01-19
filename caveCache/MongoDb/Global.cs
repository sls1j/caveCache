using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.MongoDb
{
  public class Global
  {
    public ObjectId Id { get; set; }
    public bool IsBootStrapped { get; set; }
    public int CaveNumber { get; set; }
  }
}
