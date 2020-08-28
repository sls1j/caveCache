using CaveCache.Nouns.Database;
using CaveCache.Nouns.Http;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CaveCache.Verbs
{
  public class CaveVerb
  {
    public static void Start(CaveDb db, Func<bool> isCommandLine, List<RegisterDelegate> endPoints, Func<HttpContext, Action<UserSession>, Task> handleSession)
    {
      endPoints.Get("/cave", async (ctx) =>
       {
         await handleSession(ctx, async session =>
         {
           var caves =
           db.Caves.Find(c => session.User.Caves.Contains(c.Id))
           .ToList()
           .Select(c => new
           {
             c.Id,
             Number = c.CaveNumber,
             c.Name,
             c.Description,
             c.SubType,
             CaveData = c.Data ?? new List<Data>(),
             Locations = c.Locations ?? new List<CaveLocation>(),
             Notes = c.Notes ?? new List<CaveNote>()
           })
           .ToArray();

           var response = new { Caves = caves, Status = 200 };

           ctx.WriteHeader(Mimes.Json, System.Net.HttpStatusCode.OK);

           await ctx.WriteBodyObject(response);
         });
       });

      endPoints.Put("/cave", async (ctx) =>
      {
        // upsert a cave
        await handleSession(ctx, async session =>
        {
          var request = await ctx.ReadBody<CaveUpdateRequest>();

          Cave cave = null;
          cave = db.Caves.Find(c => c.Id == request.CaveId).SingleOrDefault();
          if (null == cave)
          {
            int caveNumber = db.GetNextCaveNumber();
            cave = new Cave() { Name = $"CC #{caveNumber}", Description = string.Empty, CreatedDate = DateTime.Now, CaveNumber = caveNumber };

            db.Caves.InsertOne(cave);
            db.Users.UpdateOne(u => u.Id == session.UserId, Builders<User>.Update.AddToSet(u => u.Caves, cave.Id));
            db.History.InsertOne(UserVerb.HistoryEntry(session.UserId, cave.Id, null, null, $"Created new cave {cave.Name}:{cave.Id}"));
          }


          cave.Name = request.Name;
          cave.SubType = request.SubType;
          cave.Description = request.Description ?? string.Empty;
          cave.IsDeleted = false;

          if (request.Data != null)
          {
            cave.Data = request.Data.ToList();
          }

          if (request.Locations != null)
          {
            cave.Locations = request.Locations.ToList();
            if (cave.Locations.Count > 0 && cave.Locations.All(c => c.IsActive == false))
            {
              cave.Locations[0].IsActive = true;
            }
          }

          if (request.Notes != null)
          {
            cave.Notes = request.Notes.ToList();
          }

          db.History.InsertOne(UserVerb.HistoryEntry(session.UserId, cave.Id, null, null, $"Cave {cave.Id} updated by {session.User.Name}"));
          db.Caves.ReplaceOne(Builders<Cave>.Filter.Eq(c => c.Id, cave.Id), cave);

          // get list of all associated media
          var deadMedia = new HashSet<string>();
          var deadMediaInt = new HashSet<int>();
          foreach (var caveMedia in db.Media.AsQueryable().Where(m => m.AttachId == cave.Id && m.AttachType == "cave"))
          {
            bool notFound = true;
            var reference = $"src=\"/Media/{caveMedia.Id}\"";
            // check to see if it's contained in one of the notes
            foreach (var n in request.Notes)
            {
              if (n.Note.Contains(reference))
              {
                notFound = false;
                break;
              }
            }

            if (notFound)
            {
              if (caveMedia.OldId.HasValue)
                deadMediaInt.Add(caveMedia.OldId.Value);
              else
                deadMedia.Add(caveMedia.Id);
            }
          }

          if (deadMedia.Count > 0)
          {
            // remove the database entries
            db.Media.DeleteMany(m => deadMedia.Contains(m.Id));
          }

          var response = new { Status = HttpStatusCode.OK };
          ctx.WriteHeader(Mimes.Json, HttpStatusCode.OK);
          await ctx.WriteBodyObject(response);
        });
      });
    }
  }
}
