using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.MongoDb
{
  public class History
  {
    [BsonId]
    public ObjectId Id { get; set; }
    public ObjectId? UserId { get; set; }
    public ObjectId? CaveId { get; set; }
    public ObjectId? SurveyId { get; set; }
    public ObjectId? MediaId { get; set; }
    public DateTime EventDateTime { get; set; }
    public string Description { get; set; }
    public BsonDocument Data { get; set; }
  }
}
