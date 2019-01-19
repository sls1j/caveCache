using caveCache.MongoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDbModels
{
  class Program
  {
    static void Main(string[] args)
    {
      CaveContext db = new CaveContext("mongodb://192.168.4.10");
      Console.WriteLine($"{db.IsBootStrapped}");
      Console.ReadLine();
    }
  }
}
