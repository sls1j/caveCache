using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace caveCache.Nouns
{
  class CCEndPoint
  {
    public string Pattern;
    public EndPointType Type;
    public string Method;
    public CallbackDelegate Callback;

    public static CCEndPoint CreateExact(string pattern, CallbackDelegate callback)
    {
      return new CCEndPoint()
      {
        Pattern = pattern,
        Type = EndPointType.Exact,
        Callback = callback
      };
    }

    public static CCEndPoint CreateRoute(string pattern, CallbackDelegate callback)
    {
      return new CCEndPoint()
      {
        Pattern = pattern,
        Type = EndPointType.Route,
        Callback = callback
      };
    }
  }

  delegate void CallbackDelegate(HttpContext ctx);

  enum EndPointType { Exact, Route };  
}
