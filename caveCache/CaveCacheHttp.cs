using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using caveCache.MongoDb;
using MongoDB.Bson;

namespace caveCache
{
  class CaveCacheHttp : IDisposable
  {
    private ExecutionGuard _guard;
    private HttpListener _listener;
    private CommandRunnerMongo _cmd;
    private readonly Timer _timer;

    public CaveCacheHttp(IConfiguration conf, IMediaCache cache, CaveContext db)
    {
      JsonConvert.DefaultSettings = () =>
      {
        var settings = new JsonSerializerSettings()
        {
          Converters = new List<JsonConverter>() { new ObjectIdConverter() }
        };

        return settings;
      };

      _cmd = new CommandRunnerMongo(conf, cache, db, false);
      _guard = new ExecutionGuard();

      _listener = new HttpListener();
      _listener.Prefixes.Add("http://*:1222/");
      _listener.Start();
      _listener.BeginGetContext(HandleAPI, _listener);
      Console.WriteLine("Listening");

      _timer = new Timer(OnUpdate, null, 0, 60 * 1000);
    }

    public class ObjectIdConverter : JsonConverter
    {
      public override bool CanConvert(Type objectType)
      {
        return objectType == typeof(ObjectId);
      }

      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
      {
        if (reader.TokenType != JsonToken.String)
          throw new Exception($"Unexpected token parsing ObjectId. Expected String, got {reader.TokenType}.");

        var value = (string)reader.Value;
        return string.IsNullOrEmpty(value) ? ObjectId.Empty : new ObjectId(value);
      }

      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
      {
        if (value is ObjectId)
        {
          var objectId = (ObjectId)value;
          writer.WriteValue(objectId != ObjectId.Empty ? objectId.ToString() : string.Empty);
        }
        else
        {
          throw new Exception("Expected ObjectId value.");
        }
      }
    }

    private void OnUpdate(object state)
    {
      _guard.Execute(() =>
     {
       lock (_cmd)
         _cmd.Cleanup();
     });
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
       response.AddHeader("Access-Control-Allow-Headers", "Overwrite, Destination, Content-Type, Depth, User-Agent, Translate, Range, Content-Range, Timeout, X-File-Size, X-Requested-With, If-Modified-Since, X-File-Name, Cache-Control, Location, Lock-Token, If, CC-SessionId, CC-AttachType, CC-AttachId, CC-FileName");
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
      var request = context.Request;
      var response = context.Response;
      var raw = context.Request.RawUrl;
      Console.WriteLine($"Rcv: POST {request.RemoteEndPoint} {raw}");

      if (raw.StartsWith("/API/"))
        RunAPICommand(context);
      else if (raw == "/Media/Upload")
        Handle_SetMedia(context);
      else
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

      Console.WriteLine($"Snd: Post {request.RemoteEndPoint} {raw} RESPONSE {response.StatusCode} {response.StatusDescription}");

    }

    private void RunAPICommand(HttpListenerContext context)
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
          request = JsonConvert.DeserializeObject(json, mt, JsonConvert.DefaultSettings());
        }

        if (request != null)
        {
          object response = null;
          lock (_cmd)
            response = _cmd.GenericInvokeCommand(request);

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
      var request = context.Request;
      var response = context.Response;
      var raw = context.Request.RawUrl;
      Console.WriteLine("GET {0}", raw);

      if (raw.StartsWith("/Help"))
        Handle_GenerateHelp(context);
      else if (raw.StartsWith("/Media/"))
        Handle_GetMedia(context);
      else
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

      Console.WriteLine($"GET {request.RemoteEndPoint} {raw} RESPONSE {response.StatusCode} {response.StatusDescription}");
    }

    private void Handle_GenerateHelp(HttpListenerContext context)
    {
      API.APIHelpGenerator gen = new API.APIHelpGenerator();

      var r = context.Response;
      byte[] data = Encoding.UTF8.GetBytes(gen.HTML);
      r.OutputStream.Write(data, 0, data.Length);
      r.StatusCode = (int)HttpStatusCode.OK;
    }

    private void Handle_GetMedia(HttpListenerContext context)
    {
      var request = context.Request;
      var response = context.Response;

      var sessionId = request.Cookies["SessionId"].Value;
      var mediaIdStr = request.RawUrl.Split('/').Last();
      int? oldMediaId = null;
      ObjectId? mediaId = null;
      if (ObjectId.TryParse(mediaIdStr, out ObjectId id))
      {
        mediaId = id;
      }

      if (int.TryParse(mediaIdStr, out int iid))
      {
        oldMediaId = iid;
      }

      if (mediaId.HasValue || oldMediaId.HasValue)
      {
        // build the commandRunner command
        API.GetMediaRequest getMediaStream = new API.GetMediaRequest()
        {
          SessionId = sessionId,
          MediaId = mediaId,
          OldMediaId = oldMediaId,
        };

        // issue the request
        API.GetMediaResponse resp = null;
        lock (_cmd)
          resp = _cmd.GetMediaStream(getMediaStream);

        if (resp.Status == (int)HttpStatusCode.OK)
        {
          response.StatusCode = (int)HttpStatusCode.OK;
          try
          {
            try
            {
              byte[] buffer = new byte[4096];
              int bytesRead = 0;
              while ((bytesRead = resp.Stream.Read(buffer, 0, buffer.Length)) > 0)
                response.OutputStream.Write(buffer, 0, bytesRead);
            }
            finally
            {
              resp.Stream.Close();
            }
          }
          finally
          {
            response.OutputStream.Close();
          }

        }
        else
        {
          response.StatusCode = resp.Status;
          response.StatusDescription = resp.StatusDescription;
        }
      }
      else
      {
        response.StatusCode = (int)HttpStatusCode.BadRequest;
        response.StatusDescription = "Unable to parse the media ID.  It must be an integer or valid ObjectId string.";
      }
    }

    private void Handle_SetMedia(HttpListenerContext context)
    {
      var request = context.Request;
      var response = context.Response;

      var setMedia = new API.SetMediaRequest()
      {
        SessionId = request.Headers["CC-SessionId"],
        AttachType = request.Headers["CC-AttachType"],
        AttachId = ObjectId.Parse(request.Headers["CC-AttachId"]),
        FileName = request.Headers["CC-FileName"],
        FileSize = (int)request.ContentLength64,
        MimeType = request.ContentType,
        Media = request.InputStream,
      };

      API.SetMediaResponse setMediaResponse = null;
      lock (_cmd)
        setMediaResponse = _cmd.SetMedia(setMedia);

      response.StatusCode = setMediaResponse.Status;
      response.StatusDescription = setMediaResponse.StatusDescription;

      // write out the new media id
      if (setMediaResponse.MediaId.HasValue)
      {
        string mediaId = setMediaResponse.MediaId.ToString();
        byte[] bytes = Encoding.UTF8.GetBytes(mediaId);
        response.OutputStream.Write(bytes, 0, bytes.Length);
      }
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
