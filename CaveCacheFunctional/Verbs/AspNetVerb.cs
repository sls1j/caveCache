using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace CaveCache.Verbs
{
  public delegate void RegisterDelegate(IEndpointRouteBuilder builder);

  internal static class AspNetVerb
  {
    
    public static void Start(params RegisterDelegate[] endPointRegistrations)
    {
      IWebHostBuilder host = new WebHostBuilder()
        .UseKestrel(ko =>
       {
         ko.Listen(IPAddress.Loopback, 1222);
         ko.Limits.MaxRequestBodySize = 50 * 1024 * 1024;
         ko.Limits.MaxConcurrentConnections = 20;         
       })
        .ConfigureServices( cs =>
        {
          cs.AddRouting();
        })
        .Configure((wb, app) =>
          {
            app.UseRouting();
            app.UseEndpoints(epb =>
            {
              // register all the endpoints
              foreach (var epr in endPointRegistrations)
              {
                epr(epb);
              }
            });
          });

      host.Build().Run();
    }
  }

  public static class RouteExtensions
  {

  }
}
