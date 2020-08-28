using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaveCache.Verbs
{
  public static class EndPointExtensions
  {
    private static RequestDelegate WrapConsole(RequestDelegate callback)
    {
      RequestDelegate callback2 = async ctx =>
      {
        Console.WriteLine($"{ctx.Request.Method} {ctx.Request.Path.Value}");
        await callback(ctx);
      };

      return callback2;
    }

    public static List<RegisterDelegate> Post(this List<RegisterDelegate> endPoints, string pattern, RequestDelegate callback)
    {     
      endPoints.Add(builder => builder.MapPost(pattern, WrapConsole(callback)));
      return endPoints;
    }

    public static List<RegisterDelegate> Get(this List<RegisterDelegate> endPoints, string pattern, RequestDelegate callback)
    {
      endPoints.Add(builder => builder.MapGet(pattern, WrapConsole(callback)));
      return endPoints;
    }

    public static List<RegisterDelegate> Put(this List<RegisterDelegate> endPoints, string pattern, RequestDelegate callback)
    {
      endPoints.Add(builder => builder.MapPut(pattern, WrapConsole(callback)));
      return endPoints;
    }

    public static List<RegisterDelegate> Delete(this List<RegisterDelegate> endPoints, string pattern, RequestDelegate callback)
    {
      endPoints.Add(builder => builder.MapDelete(pattern, WrapConsole(callback)));
      return endPoints;
    }
  }
}
