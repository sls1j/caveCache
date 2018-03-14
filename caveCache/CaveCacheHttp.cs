using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace caveCache
{
    class CaveCacheHttp : IDisposable
    {
        private ExecutionGuard _guard;
        private HttpListener _listener;
        private CommandRunner _cmd;

        public CaveCacheHttp( IConfiguration conf, IMediaCache cache, Database.CaveCacheContext db )
        {
            _cmd = new CommandRunner(conf, cache, db, false);
            _guard = new ExecutionGuard();

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:1222/");
            _listener.Start();
            _listener.BeginGetContext(HandleAPI, _listener);
            Console.WriteLine("Listening");
        }

        private void HandleAPI(IAsyncResult ar)
        {
            _guard.Execute(() =>
           {
               var listener = ar.AsyncState as HttpListener;
               var context = listener.EndGetContext(ar);

                // allows handling multiple requests at the same time
                listener.BeginGetContext(HandleAPI, listener);

                // allows for cross-domain access from a script.
                var response = context.Response;
               var request = context.Request;
               response.AddHeader("Access-Control-Allow-Origin", "*");
               response.AddHeader("Access-Control-Allow-Credentials", "true");
               response.AddHeader("Access-Control-Allow-Headers", "Overwrite, Destination, Content-Type, Depth, User-Agent, Translate, Range, Content-Range, Timeout, X-File-Size, X-Requested-With, If-Modified-Since, X-File-Name, Cache-Control, Location, Lock-Token, If");
               response.AddHeader("Access-Control-Allow-Methods", "DELETE, GET, MOVE, OPTIONS, POST, PUT, UPDATE");
               response.AddHeader("Access-Control-Expose-Headers", "DAV, content-length, Allow");
               response.AddHeader("Access-Control-Max-Age", "1728000");

               switch (context.Request.HttpMethod)
               {
                   case "GET":
                       HandleGetAPI(context);
                       break;
                   case "POST":
                       HandlePostAPI(context);
                       break;
                   case "OPTIONS":
                       response.StatusCode = (int)HttpStatusCode.OK;
                       break;
                   default:
                       response.StatusCode = (int)HttpStatusCode.NotFound;
                       break;
               }

               response.Close();
           });
        }

        private void HandleOptionsAPI(HttpListenerContext context)
        {

        }

        private void HandlePostAPI(HttpListenerContext context)
        {
            Console.WriteLine("POST {0}", context.Request.RawUrl);

            if (context.Request.RawUrl.StartsWith("/API/"))
            {
                RunCommand(context);
            }
            else
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            Console.WriteLine("POST {0} RESPONSE {1}", context.Request.RawUrl, context.Response.StatusCode);
        }

        private void RunCommand(HttpListenerContext context)
        {
            try
            {
                // get type of message
                string typeName = "caveCache.API." + context.Request.RawUrl.Substring(5);
                Type mt = Type.GetType(typeName, false, true);
                // get the command info
                object request = null;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    string json = reader.ReadToEnd();
                    request = JsonConvert.DeserializeObject(json, mt);
                }

                if (request != null)
                {
                    var response = _cmd.GenericInvokeCommand(request);
                    if (response != null)
                    {
                        string json = JsonConvert.SerializeObject(response);
                        byte[] buffer = Encoding.UTF8.GetBytes(json);
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.StatusDescription = "OK";
                        context.Response.ContentType = "application/json";
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        context.Response.StatusDescription = "Not found";
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.StatusDescription = "Not found";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType().Name} {ex.Message}\r\n{ex.StackTrace}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                //context.Response.StatusDescription = $"Internal Error: {ex.Message} {ex.StackTrace}";
            }
        }

        private void HandleGetAPI(HttpListenerContext context)
        {
            Console.WriteLine("GET {0}", context.Request.RawUrl);

            if (context.Request.RawUrl.StartsWith("/Media/"))
                Handle_Media(context);
            else
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            Console.WriteLine("GET {0} RESPONSE {1}", context.Request.RawUrl, context.Response.StatusCode);
        }

        private void Handle_Media(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            // build the commandRunner command
            // issue the request
        }

        private void API_Test(HttpListenerContext context)
        {
            var response = context.Response;
            string[] testData = new[] { "one", "two", "three" };
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(testData));
            response.ContentType = "text/plain";
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
            Console.WriteLine("Responsed to Request");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool isDisposing)
        {
            if (_guard.DisableExecute())
            {
                if (isDisposing)
                {
                    if (null != _listener)
                        _listener.Close();
                }

                _listener = null;
            }
        }
    }
}
