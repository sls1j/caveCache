using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;
using caveCache.Nouns;
using System.Linq;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace caveCache
{
  internal static class AspNetVerb
  {
    public static void Start(Action<IEndpointRouteBuilder>[] endPointRegistrations)
    {
      IWebHostBuilder host = new WebHostBuilder()
        .UseKestrel(ko =>
       {
         ko.ListenLocalhost(1222);
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
