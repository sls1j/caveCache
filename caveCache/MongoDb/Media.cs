using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.MongoDb
{
  public class Media
  {
    [BsonId]
    public ObjectId Id { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
    public int FileSize { get; set; }
    public ObjectId AttachId { get; set; }
    public string AttachType { get; set; }
    public int? OldId { get; set; }

    public Media Clone()
    {
      return MemberwiseClone() as Media;
    }
  }
}
