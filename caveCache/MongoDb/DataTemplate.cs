using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.MongoDb
{
  public class DataTemplate
  {
    [BsonId]
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<DataTemplateItem> Items { get; set; }
  }

  public class DataTemplateItem
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string DefaultValue { get; set; }
    public string Control { get; set; }
  }
}
