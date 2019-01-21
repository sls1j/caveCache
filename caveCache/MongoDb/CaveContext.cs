using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace caveCache.MongoDb
{
  public class CaveContext
  {
    private const string DatabaseName = "CaveCache";
    private MongoClient _client;
    private IMongoDatabase _db;
    private string _connectionString;

    public CaveContext(string connectionString)
    {
      Console.WriteLine($"Connecting to '{connectionString}'");
      _connectionString = connectionString;
      _client = new MongoClient(_connectionString);
      _db = _client.GetDatabase(DatabaseName);
      GetGlobal();
    }

    public string ConnectionString { get => _connectionString; }
    public IMongoCollection<Cave> Caves { get => _db.GetCollection<Cave>("Caves"); }
    public IMongoCollection<User> Users { get => _db.GetCollection<User>("Users"); }
    public IMongoCollection<UserSession> UserSessions { get => _db.GetCollection<UserSession>("UserSessions"); }
    public IMongoCollection<Group> Groups { get => _db.GetCollection<Group>("Groups"); }
    public IMongoCollection<History> History { get => _db.GetCollection<History>("History"); }
    public IMongoCollection<Media> Media { get => _db.GetCollection<Media>("Media"); }
    public IMongoCollection<DataTemplate> DataTemplate { get => _db.GetCollection<DataTemplate>("DataTemplate"); }
    public IMongoCollection<Global> Globals { get => _db.GetCollection<Global>("Globals"); }

    public bool IsBootStrapped
    {
      get => Globals.Find(g => true).Project(g => g.IsBootStrapped).Single();
      set { Globals.UpdateOne(g => true, Builders<Global>.Update.Set(g => g.IsBootStrapped, value)); }
    }
    public int GetNextCaveNumber()
    {
      int returnValue = Globals.Find(g => true).Project(g => g.CaveNumber).Single();

      Globals.UpdateOne(g => true, Builders<Global>.Update.Set(g => g.CaveNumber, returnValue));
      return returnValue;
    }

    private void GetGlobal()
    {
      var col = _db.GetCollection<Global>("Globals");
      var global = col.AsQueryable().FirstOrDefault();
      if (null == global)
      {
        global = new Global();
        global.IsBootStrapped = false;
        global.CaveNumber = 1;
        col.InsertOne(global);
      }
    }

  }
}
