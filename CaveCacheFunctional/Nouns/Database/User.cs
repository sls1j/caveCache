using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaveCache.Nouns.Database
{
  public class User
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastLoggedIn { get; set; }
    public DateTime? Expire { get; set; }
    public string Profile { get; set; }
    public string Permissions { get; set; }
    public string PasswordSalt { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; }


    public List<Data> Data { get; set; }
    public List<string> Caves { get; set; }

    public User()
    {
      Data = new List<Data>();
      Caves = new List<string>();
    }

    public override string ToString()
    {
      return string.Format("{0}:{1}", Name, Id);
    }
  }
}
