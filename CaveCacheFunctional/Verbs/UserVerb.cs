using CaveCache.Nouns.Database;
using CaveCache.Nouns.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CaveCache.Verbs
{
    internal class UserVerb
    {
        public static void Start(CaveDb db, Func<bool> isCommandLine, List<RegisterDelegate> endPoints, out Func<HttpContext, Action<UserSession>, Task> handleSession)
        {
            RandomNumberGenerator rng = RandomNumberGenerator.Create();

            Func<int, byte[]> getRng = cnt =>
             {
                 byte[] data = new byte[cnt];
                 rng.GetBytes(data);
                 return data;
             };

            Func<HttpContext, Action<UserSession>, Task> innerHandleSession = BuildHandleSession(db, isCommandLine);

            handleSession = innerHandleSession;

            Task handleResponse<T>(HttpContext ctx, Func<UserSession, T, Task> action) where T : Response, new()
            {
                return innerHandleSession(ctx, async session =>
                {
                    T response = new T();
                    response.SetSuccess();
                    try
                    {
                        await action(session, response);
                    }
                    catch (Exception ex)
                    {
                        response.SetFail(HttpStatusCode.InternalServerError, $"Exception thrown: {ex.Message} \r\n {ex.StackTrace}");
                    }

                    ctx.WriteHeader(Mimes.Json, HttpStatusCode.OK);
                    await ctx.WriteBodyObject(response);
                });
            }

            // define endpoints
            endPoints.Post("/login", async ctx =>
              {
                  // read body
                  var login = await ctx.ReadBody<LoginRequest>();

                  // get login
                  var loginResponse = Login(db, isCommandLine, getRng, login);

                  // return body
                  ctx.WriteHeader(Mimes.Json, HttpStatusCode.OK);
                  await ctx.WriteBodyObject(loginResponse);
              });

            endPoints.Get("/user/profile", async ctx =>
            {
                await innerHandleSession(ctx, async session =>
                {
                    ctx.WriteHeader(Mimes.Json, HttpStatusCode.OK);
                    await ctx.WriteBodyObject(new
                    {
                        session.UserId,
                        session.User.Email,
                        session.User.Profile,
                        session.User.Permissions,
                        Status = 200
                    });
                });
            });

            endPoints.Get("/user", async ctx =>
            {
                await handleResponse<ItemsResponse<User>>(ctx, async (session, response) =>
                {
                    if (!session.User.Permissions.Contains('A'))
                    {
                        ctx.Unauthorized();
                        return;
                    }

                    var users = db.Users.Find(u => true).ToList().ToArray();
                    response.Items = users;
                });
            });

            endPoints.Put("/user", async ctx =>
            {
                await handleResponse<Response>(ctx, async (session, response) =>
                {
                    if (!session.User.Permissions.Contains('A'))
                    {
                        ctx.Unauthorized();
                        return;
                    }

                    var request = await ctx.ReadBody<UserUpdateRequest>();

                    var user = db.Users.Find(u => u.Id == request.Id).FirstOrDefault();

                    if (user == null)
                    {
                        user = new User();
                        user.Id = Guid.NewGuid().ToString("n");
                        db.Users.InsertOne(user);
                    }

                    user.Name = request.Name;
                    user.Email = request.Email;
                    user.Profile = request.Profile;
                    user.Permissions = request.Permissions;

                    db.Users.ReplaceOne(u => u.Id == user.Id, user);
                });
            });

            endPoints.Put("/user/new", async ctx =>
            {
                await handleResponse<ItemResponse<NewUser>>(ctx, async (session, response) =>
                {
                    UserNewRequest request = await ctx.ReadBody<UserNewRequest>();

                    if (string.IsNullOrWhiteSpace(request.Email))
                    {
                        response.SetFail("No email address specified");
                        return;
                    }

                    if (db.Users.Find(u => u.Email == request.Email).Any())
                    {
                        response.SetFail("Email already exists.");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(request.Password))
                    {
                        request.Password = GenerateUserPassword();
                    }

                    User user = new User()
                    {
                        Name = request.Name,
                        Email = request.Email,
                        IsActive = true,
                        Created = DateTime.UtcNow,
                        Profile = request.Profile,
                        Permissions = request.Permissions,
                        PasswordSalt = Convert.ToBase64String(getRng(16)),
                    };

                    user.PasswordHash = HashPassword(request.Password, user.PasswordSalt);

                    db.Users.InsertOne(user);

                    response.Item = new NewUser()
                    {
                        Email = user.Email,
                        Id = user.Id,
                        Name = user.Name,
                        Password = request.Password,
                        Permissions = user.Permissions,
                        Profile = user.Profile
                    };

                });
            });

            endPoints.Delete("/user/{id}", async ctx =>
            {
                await handleResponse<Response>(ctx, async (session, response) =>
                {
                    string id = ctx.Request.RouteValues["id"].ToString();

                    if (db.Users.DeleteOne(u => u.Id == id).DeletedCount == 0)
                    {
                        response.SetFail("$No user with id {id}");
                        return;
                    }

                    db.History.InsertOne(HistoryEntry(id, null, null, null, "User Deleted"));
                });
            });

            endPoints.Get("/user/password/reset/{id}", async ctx =>
            {
                await handleResponse<UserResetPasswordResponse>(ctx, async (session, response) =>
                {
                    string id = ctx.Request.RouteValues["id"].ToString();
                    User user = db.Users.Find(u => u.Id == id).FirstOrDefault();

                    if (user == null)
                    {
                        response.SetFail($"User {id} not found.");
                        return;
                    }

                    response.NewPassword = GenerateUserPassword();
                    byte[] salt = getRng(16);
                    user.PasswordSalt = Convert.ToBase64String(salt);
                    user.PasswordHash = HashPassword(response.NewPassword, user.PasswordSalt);
                    db.Users.ReplaceOne(u => u.Id == user.Id, user);
                });
            });
        }

        readonly static string[] words = new[]{
               "accident","ascender","bacon","bag","bat","bolt", "bone","boot",
               "boulder","break","bridge", "cave","caver","ceiling","climb","clinometer",
               "compass","crack","crawl","cricket","dark","descender","eat","explosive",
               "fall","fatal","floor","flower","flowstone","gloves","hand", "helmet","hole","jump","karst", "knot",
               "lamp", "laser","light","light","log","millipede","mud", "pad","pencil","pit","rat","river",
               "rock", "rope","sleep","sloth","snake","sprain","squeeze","suit","tape","tree"};

        private static string GenerateUserPassword()
        {
            Random r = new Random();

            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            for (int i = 0; i < 4; i++)
            {
                if (isFirst)
                    isFirst = false;
                else sb.Append(" ");
                sb.Append(words[r.Next(words.Length)]);
            }

            return sb.ToString();
        }

        public static LoginResponse Login(CaveDb db, Func<bool> isCommandLine, Func<int, byte[]> getRandomBytes, LoginRequest request)
        {
            // find user
            var user = db.Users.Find(u => u.Email == request.Email).FirstOrDefault();
            if (null == user)
            {
                HistoryEntry(null, null, null, null, "Failed login for bad user {0}", request.Email);
                return new LoginResponse() { Status = (int)HttpStatusCode.Unauthorized, StatusDescription = "Username or Password is incorrect" };
            }

            // only allow the admin password via the command line
            if (user.Email == "admin" && !isCommandLine())
            {
                HistoryEntry(null, null, null, null, "Failed login for bad user {0}", request.Email);
                return new LoginResponse() { Status = (int)HttpStatusCode.Unauthorized, StatusDescription = "Username or Password is incorrect" };
            }

            // verify password
            var hash = HashPassword(request.Password, user.PasswordSalt);
            if (hash != user.PasswordHash)
            {
                HistoryEntry(null, null, null, null, "Failed login for user {0}. Bad password.", user);
                return new LoginResponse() { Status = (int)HttpStatusCode.Unauthorized, StatusDescription = "Username or Password is incorrect" };
            }

            var sessionKey = getRandomBytes(8);

            var now = DateTime.UtcNow;
            // create session
            UserSession session = new UserSession()
            {
                Id = Guid.NewGuid().ToString(),
                IsCommandLine = isCommandLine(),
                SessionId = Convert.ToBase64String(sessionKey),
                Timeout = now.AddHours(1),
                UserId = user.Id,
                User = user
            };

            var deadSessions = db.UserSessions.DeleteMany(s => s.Timeout < now);
            db.UserSessions.InsertOne(session);
            db.Users.UpdateOne(u => u.Id == session.User.Id, Builders<User>.Update.Set(u => u.LastLoggedIn, now));
            db.History.InsertOne(HistoryEntry(user.Id, null, null, null, "User {0} logged in.", user));

            return new LoginResponse()
            {
                SessionId = session.SessionId,
                UserId = user.Id,
                Name = user.Name,
                Profile = user.Profile,
                Permissions = user.Permissions,
                Status = (int)HttpStatusCode.OK
            };
        }

        public static Func<HttpContext, Action<UserSession>, Task> BuildHandleSession(CaveDb db, Func<bool> isCommandLine)
        {
            return async (ctx, handler) =>
            {
                string sessionId = ctx.Request.Headers["Authorization"];

                UserSession session = null;

                bool isExpired = false;
                do
                {
                    var sessionQuery = db.UserSessions.AsQueryable();
                    if (isCommandLine())
                        session = sessionQuery.FirstOrDefault(s => s.IsCommandLine);
                    else
                        session = sessionQuery.FirstOrDefault(s => s.SessionId == sessionId);


                    if (null != session && session.Timeout < DateTime.UtcNow)
                    {
                        db.UserSessions.DeleteOne(s => s.Id == session.Id);
                        isExpired = true;
                    }
                    else
                        isExpired = false;
                }
                while (isExpired);

                if (session != null)
                {
                    session.Timeout = DateTime.UtcNow.AddHours(1);
                    db.UserSessions.UpdateOne(s => s.Id == session.Id, Builders<UserSession>.Update.Set(s => s.Timeout, session.Timeout));
                    session.User = db.Users.Find(u => u.Id == session.UserId).First();
                    handler(session);
                }
                else
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
            };
        }

        public static History HistoryEntry(string UserId, string CaveId, string SurveyId, string MediaId, string Description, params object[] args)
        {
            string fullDescription;
            if (args == null || args.Length == 0)
                fullDescription = Description;
            else
                fullDescription = string.Format(Description, args);

            var history = new History()
            {
                Id = Guid.NewGuid().ToString("n"),
                UserId = UserId,
                CaveId = CaveId,
                SurveyId = SurveyId,
                MediaId = MediaId,
                EventDateTime = DateTime.UtcNow,
                Description = fullDescription,
            };


            return history;
        }

        private static string HashPassword(string password, string salt)
        {
            using (var hasher = new SHA256Managed())
            {
                var a = password + salt;
                var abytes = Encoding.UTF8.GetBytes(a);
                var hash = Convert.ToBase64String(hasher.ComputeHash(abytes));
                return hash;
            }
        }

    }
}
