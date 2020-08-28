using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaveCache.Nouns.Database;
using CaveCache.Verbs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CaveCache
{
  public class Program
  {
    public static void Main(string[] args)
    {
      List<RegisterDelegate> endPoints = new List<RegisterDelegate>();
      CaveDb db = new CaveDb("mongodb://192.168.5.10");



      Func<HttpContext, Action<UserSession>, Task> handleSession;
      Func<bool> isCommandLine = () => false;
      UserVerb.Start(db, isCommandLine, endPoints, out handleSession);
      CaveVerb.Start(db, isCommandLine, endPoints, handleSession);

      AspNetVerb.Start(endPoints.ToArray());
    }
  }
}

