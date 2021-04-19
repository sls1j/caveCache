using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CaveCache.Verbs
{
    public static class HttpContextExtensions
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() { new StringEnumConverter() },
            Formatting = Formatting.Indented
        };

        public static void Unauthorized(this HttpContext ctx)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }

        public async static Task<T> ReadBody<T>(this HttpContext ctx)
        {
            string json = await ctx.ReadBodyAsString();
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public async static Task<string> ReadBodyAsString(this HttpContext ctx)
        {
            byte[] data = await ctx.ReadBodyAsBytes();
            return Encoding.UTF8.GetString(data);
        }

        public async static Task<byte[]> ReadBodyAsBytes(this HttpContext ctx)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                await ctx.Request.Body.CopyToAsync(stream);
                return stream.ToArray();
            }
        }        

      public static HttpContext WriteHeader(this HttpContext ctx, string contentType, HttpStatusCode status)
        {
            ctx.Response.ContentType = contentType;
            ctx.Response.StatusCode = (int)status;
            return ctx;
        }

        public async static Task WriteBodyObject(this HttpContext ctx, object body)
        {
            string json = JsonConvert.SerializeObject(body, settings);
            await ctx.WriteBodyString(json);
        }

        public async static Task WriteBodyString(this HttpContext ctx, string body)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(body);
            await ctx.WriteBodyBytes(buffer);
        }

        public async static Task WriteBodyBytes(this HttpContext ctx, byte[] body)
        {
            await ctx.Response.Body.WriteAsync(body, 0, body.Length);
        }
    }
}
