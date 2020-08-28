using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaveCache.Nouns.Database
{
  public class History
  {
    [BsonId]
    public string Id { get; set; }
    public string? UserId { get; set; }
    public string? CaveId { get; set; }
    public string? SurveyId { get; set; }
    public string? MediaId { get; set; }
    public DateTime EventDateTime { get; set; }
    public string Description { get; set; }
    public BsonDocument Data { get; set; }
  }
}
