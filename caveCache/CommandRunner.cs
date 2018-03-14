using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using caveCache.Database;
using System.Net;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace caveCache
{
    class CommandRunner
    {
        private Database.CaveCacheContext _db;
        private RandomNumberGenerator _rng;
        private IConfiguration _config;
        private IMediaCache _cache;
        private bool _isCommandLine;
        private Dictionary<Type, Func<object, object>> _commands;

        public CommandRunner(IConfiguration config, IMediaCache cache, CaveCacheContext db, bool isCommandLine = false)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _cache = cache ?? throw new ArgumentNullException(nameof(config));
            _db = db ?? throw new ArgumentNullException(nameof(db));


            this._isCommandLine = isCommandLine;            

            Console.WriteLine($"Database at: {_db.ConnectionString}");

            _rng = RandomNumberGenerator.Create();

            _commands = new Dictionary<Type, Func<object, object>>();

            foreach (var method in this.GetType().GetMethods())
            {
                var pars = method.GetParameters();

                if (pars.Length == 1
                    && pars[0].ParameterType.IsSubclassOf(typeof(API.Request))
                    && method.ReturnType.IsSubclassOf(typeof(API.Response)))
                {
                    Type requestType = pars[0].ParameterType;
                    _commands[requestType] = (request) =>
                    {
                        var response = method.Invoke(this, new object[] { request });
                        return response;
                    };
                    Console.WriteLine("Added command for {0}", requestType.Name);
                }
            }

            BootStrap();
        }

        public void BootStrap()
        {
            // check that the boot strap hasn't already run            
            var isBootstrapped = _db.Globals.FirstOrDefault(i => i.Key == Globals.IsBootStrapped);
            if (isBootstrapped != null && isBootstrapped.Value == true.ToString())
            {
                Console.WriteLine("Already boot strapped");
                return;
            }

            // create admin user
            User user = new User()
            {
                Created = DateTime.Now,
                Email = "admin",
                Expire = null,
                Name = "Admin",
                Permissions = "admin",
                Profile = ""                
            };

            string password = "password";
            GenerateUserPasswordHash(user, password);

            isBootstrapped = new Global() { Key = Globals.IsBootStrapped, Value = true.ToString() };


            _db.Users.Add(user);
            _db.Globals.Add(isBootstrapped);
            _db.SaveChanges(true);

            Console.WriteLine("System is ready for use.");
        }

        private History HistoryEntry(int? UserId, int? CaveId, int? SurveyId, int? MediaId, string Description, params object[] args)
        {
            string fullDescription;
            if (args == null || args.Length == 0)
                fullDescription = Description;
            else
                fullDescription = string.Format(Description, args);

            var history = new History()
            {
                UserId = UserId,
                CaveId = CaveId,
                SurveyId = SurveyId,
                MediaId = MediaId,
                EventDateTime = DateTime.UtcNow,
                Description = fullDescription,
                Data = ""
            };


            return history;
        }

        public string GetAdminSession()
        {
            var session = _db.Sessions.FirstOrDefault(s => s.UserId == 1);
            if (session != null)
                return session.SessionId;
            else
                return "";
        }

        public API.LoginResponse Login(API.LoginRequest request)
        {
            // find user
            var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (null == user)
            {
                HistoryEntry(null, null, null, null, "Failed login for bad user {0}", request.Email);
                return new API.LoginResponse() { RequestId = request.RequestId, Status = (int)HttpStatusCode.Unauthorized, Error = "Username or Password is incorrect" };
            }

            // verify password
            var hash = HashPassword(request.Password, user.PasswordSalt);
            if (hash != user.PasswordHash)
            {
                HistoryEntry(null, null, null, null, "Failed login for user {0}. Bad password.", user);
                return new API.LoginResponse() { RequestId = request.RequestId, Status = (int)HttpStatusCode.Unauthorized, Error = "Username or Password is incorrect" };
            }

            var sessionKey = new byte[8];
            _rng.GetBytes(sessionKey);

            var now = DateTime.UtcNow;
            // create session
            UserSession session = new UserSession()
            {
                IsCommandLine = _isCommandLine,
                SessionId = Convert.ToBase64String(sessionKey),
                Timeout = now.AddHours(1),
                UserId = user.UserId
            };

            var deadSessions = _db.Sessions.Where(s => s.Timeout < now).ToList();
            foreach (var s in deadSessions)
                _db.Sessions.Remove(s);
            _db.Sessions.Add(session);
            _db.History.Add(HistoryEntry(user.UserId, null, null, null, "User {0} logged in.", user));
            _db.SaveChanges();

            return new API.LoginResponse()
            {
                RequestId = request.RequestId,
                SessionId = session.SessionId,
                Status = (int)HttpStatusCode.OK
            };
        }


        private void GenerateUserPasswordHash(User user, string password)
        {
            // generate salt
            var saltBytes = new byte[32];

            _rng.GetBytes(saltBytes);
            user.PasswordSalt = Convert.ToBase64String(saltBytes);

            // concatinate salt + password
            user.PasswordHash = HashPassword(password, user.PasswordSalt);
        }

        private string HashPassword(string password, string salt)
        {
            using (var hasher = new SHA256Managed())
            {
                var a = password + salt;
                var abytes = Encoding.UTF8.GetBytes(a);
                var hash = Convert.ToBase64String(hasher.ComputeHash(abytes));
                return hash;
            }
        }

        private T Fail<T>(API.SessionRequest request, HttpStatusCode code, string errorMessage) where T : API.SessionResponse, new()
        {
            var response = new T();
            response.RequestId = request.RequestId;
            response.SessionId = request.SessionId;
            response.Status = (int)code;
            response.Error = errorMessage;
            return response;
        }

        public API.UserAddResponse AddUser(API.UserAddRequest request)
        {
            // look for session
            var session = FindSession(request.SessionId);
            if (!session.User.Permissions.Contains("admin"))
            {
                HistoryEntry(session.User.UserId, null, null, null, "{0} Failed to create user {1}.  Not authorized", session.User, request.Email);
                return Fail<API.UserAddResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");
            }

            // check that there isn't already a user
            if (_db.Users.Any(u => string.Equals(u.Email, request.Email, StringComparison.CurrentCultureIgnoreCase)))
            {
                _db.History.Add(HistoryEntry(session.User.UserId, null, null, null, "{0}Failed to create user with email {0}.  Already exists", session.User.Name, request.Email));
                _db.SaveChanges();
                return Fail<API.UserAddResponse>(request, HttpStatusCode.BadRequest, "User already exists.");
            }

            // add user to database
            if (string.IsNullOrEmpty(request.Password))
                request.Password = GenerateUserPassword();

            var user = new User()
            {
                Created = DateTime.UtcNow,
                Email = request.Email,
                Expire = DateTime.UtcNow.AddYears(1),
                LastLoggedIn = null,
                Name = request.Name,
                Permissions = "",
                Profile = request.Profile,
            };

            GenerateUserPasswordHash(user, request.Password);

            _db.SaveChanges();

            HistoryEntry(session.UserId, null, null, null, "{0} Created user {1}", session.User, user);

            // return user object
            return new API.UserAddResponse()
            {
                Password = request.Password,
                RequestId = request.RequestId,
                SessionId = request.SessionId,
                Status = (int)HttpStatusCode.OK
            };
        }

        private string GenerateUserPassword()
        {
            string[] words = new[]{
                "cave", "rope", "hole", "karst", "bat",
                "flowstone", "bacon", "caver", "pit", "river",
                "ascender", "descender", "squeeze", "ceiling", "floor",
                "sloth", "snake", "cricket", "millipede", "rat",
                "dark", "light", "boot", "suit", "compass",
                "tree", "boulder", "rock", "log", "bone",
                "crack", "explosive", "gloves", "bag", "rope",
                "fall", "break", "fatal", "accident", "sprain",
                "pencil", "pad", "laser", "clinometer", "tape",
                "crawl", "climb", "jump", "eat", "sleep"};

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

        UserSession Combine(UserSession session, User u)
        {
            session.User = u;
            return session;
        }


        private UserSession FindSession(string sessionId)
        {
            
            UserSession session = null;

            bool isExpired = false;
            do
            {
                var sessionQuery =
                    from s in _db.Sessions
                    join u in _db.Users.Include(t => t.Data) on s.UserId equals u.UserId
                    select Combine( s, u );

                if (_isCommandLine)
                    session = sessionQuery.FirstOrDefault(s => s.IsCommandLine);
                else
                    session = sessionQuery.FirstOrDefault(s => s.SessionId == sessionId);


                if (null != session && session.Timeout < DateTime.UtcNow)
                {
                    _db.Sessions.Remove(session);
                    _db.SaveChanges();
                    isExpired = true;
                }
                else
                    isExpired = false;
            }
            while (isExpired);

            if (session != null)
            {
                session.Timeout = DateTime.UtcNow.AddHours(1);
                _db.SaveChanges();
            }

            return session;
        }

        public API.UserListResponse UserList(API.UserListRequest request)
        {
            var session = FindSession(request.SessionId);
            if (session == null || !session.User.Permissions.Contains("admin"))
                return Fail<API.UserListResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

            var response = new API.UserListResponse() { RequestId = request.RequestId, SessionId = request.SessionId, Status = (int)HttpStatusCode.OK };

            response.Users = _db.Users.ToArray();
            return response;
        }

        public API.SessionListResponse SessionList(API.SessionListRequest request)
        {
            var session = FindSession(request.SessionId);
            if (session == null || !session.User.Permissions.Contains("admin"))
                return Fail<API.SessionListResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

            var response = new API.SessionListResponse() { RequestId = request.RequestId, SessionId = request.SessionId, Status = (int)HttpStatusCode.OK };

            response.Sessions = _db.Sessions.ToArray();
            return response;
        }

        public API.UserResetPasswordResponse ResetPassword(API.UserResetPasswordRequest request)
        {
            var session = FindSession(request.SessionId);
            if (session == null || !session.User.Permissions.Contains("admin"))
                return Fail<API.UserResetPasswordResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

            // find the user
            var user = _db.Users.FirstOrDefault(u => string.Equals(u.Email, request.Email, StringComparison.CurrentCultureIgnoreCase));
            if (null == user)
                return Fail<API.UserResetPasswordResponse>(request, HttpStatusCode.NotFound, "No user with matching email found.");

            var newPassword = GenerateUserPassword();
            GenerateUserPasswordHash(user, newPassword);

            _db.History.Add(HistoryEntry(session.User.UserId, null, null, null, "Reset user {0} password", user));
            _db.History.Add(HistoryEntry(user.UserId, null, null, null, "Password was reset by {0}", session.User));
            _db.SaveChanges();


            return new API.UserResetPasswordResponse()
            {
                Status = (int)HttpStatusCode.OK,
                RequestId = request.RequestId,
                SessionId = request.SessionId,
                NewPassword = newPassword
            };
        }

        public API.CaveAddUpdateResponse CaveAddUpdate(API.CaveAddUpdateRequest request)
        {
            var session = FindSession(request.SessionId);
            if (session == null)
                return Fail<API.CaveAddUpdateResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

            if (string.IsNullOrEmpty(request.Name))
                return Fail<API.CaveAddUpdateResponse>(request, HttpStatusCode.BadRequest, "Cave must have a name.");

            bool isNew = false;
            Cave cave = null;
            if (request.CaveId.HasValue)
            {
                cave = _db.Caves.FirstOrDefault(c => c.CaveId == request.CaveId.Value);
                if (null == cave)
                    return Fail<API.CaveAddUpdateResponse>(request, HttpStatusCode.BadRequest, "Cannot update a non-existant cave.");
            }
            else
            {
                cave = new Cave();
                isNew = true;
            }

            if (!isNew)
            {
                cave.LocationId = null;
                _db.CaveLocations.RemoveRange(_db.CaveLocations.Where(cl => cl.CaveId == cave.CaveId).ToArray());
                _db.CaveData.RemoveRange(_db.CaveData.Where(cd => cd.CaveId == cave.CaveId).ToArray());
                _db.CaveUsers.Remove(_db.CaveUsers.Where(cu => cu.CaveId == cave.CaveId && cu.UserId == session.UserId).First());
                _db.SaveChanges();
            }

            cave.Name = request.Name;
            cave.Description = request.Description ?? string.Empty;
            cave.IsDeleted = false;
            _db.Caves.Add(cave);

            _db.SaveChanges();

            if (request.Data != null)
            {
                _db.CaveData.AddRange(request.Data.Select(d => new CaveData() { CaveId = cave.CaveId, Name = d.Name, Type = d.Type, MetaData = d.MetaData??string.Empty, Value = d.Value }));
            }            

            var caveUser = new CaveUser()
            {
                CaveId = cave.CaveId,
                UserId = session.UserId
            };

            _db.CaveUsers.Add(caveUser);

            foreach (var l in request.Locations)
            {
                var location = new CaveLocation()
                {
                    CaveId = cave.CaveId,
                    LocationId = l.LocationId,
                    Accuracy = l.Accuracy,
                    Altitude = l.Altitude,
                    AltitudeAccuracy = l.AltitudeAccuracy,
                    CaptureDate = l.CaptureDate,
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Notes = l.Notes??string.Empty,
                    Source = l.Source??string.Empty,
                    Unit = l.Unit??"Emperial",
                };

                _db.CaveLocations.Add(location);
            }

            if (isNew)
                _db.History.Add(HistoryEntry(session.UserId, cave.CaveId, null, null, "Cave added by {0}", session.User.Name));
            else
                _db.History.Add(HistoryEntry(session.UserId, cave.CaveId, null, null, "Cave updated by {0}", session.User.Name));

            _db.SaveChanges();

            return new API.CaveAddUpdateResponse()
            {
                RequestId = request.RequestId,
                SessionId = request.SessionId,
                CaveId = cave.CaveId,
                Status = (int)HttpStatusCode.OK
            };
        }

        public API.CaveListResponse CaveList(API.CaveListRequest request)
        {
            var session = FindSession(request.SessionId);
            if (session == null)
                return Fail<API.CaveListResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

            API.CaveListResponse response = new API.CaveListResponse() { RequestId = request.RequestId, SessionId = request.SessionId, Status = (int)HttpStatusCode.OK };
            var caveQuery = _db.Caves.Include(t => t.Data).Include(t => t.Locations);

            if (session.User.Permissions.Contains("admin") && request.allCaves)
                response.Caves = caveQuery.ToArray();
            else
            {
                var caves =
                    from c in caveQuery
                    join cu in _db.CaveUsers.Include(t => t.Cave) on c.CaveId equals cu.CaveId
                    where cu.User == session.User
                    select c;

                response.Caves = caves.ToArray();
            }

            return response;
        }

        public API.UserGetInfoResponse UserGetInfo(API.UserGetInfoRequest request)
        {
            var session = FindSession(request.SessionId);
            if (session == null)
                return Fail<API.UserGetInfoResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

            var user = session.User;

            API.UserGetInfoResponse response = new API.UserGetInfoResponse()
            {
                RequestId = request.RequestId,
                SessionId = request.SessionId,
                Name = user.Name,
                Profile = user.Profile,
                Permissions = user.Permissions,
                Status = (int)HttpStatusCode.OK,
                Data = user.Data.Select(d => d.Clone()).ToArray()
            };

            // media data
            response.Media =
             (from m in _db.UserMedia.Include(t => t.Media)
              where m.UserId == session.UserId
              select new API.MediaMetaData()
              {
                  MediaId = m.MediaId,
                  Name = m.Media.Name,
                  Description = m.Media.Description,
                  FileName = m.Media.FileName,
                  FileSize = m.Media.FileSize,
                  MimeType = m.Media.MimeType
              }).ToArray();

            // get survey data
            // TODO

            // get cave data
            response.Caves =
               (from cu in _db.CaveUsers.Include(t => t.Cave)
                join cd in _db.CaveData on cu.CaveId equals cd.CaveId into caveData
                where cu.UserId == user.UserId
                select new API.CaveInfoShort()
                {
                    CaveId = cu.CaveId,
                    LocationId = cu.Cave.LocationId,
                    Locations = cu.Cave.Locations.ToArray(),
                    Description = cu.Cave.Description,
                    Name = cu.Cave.Name,
                    CaveData = (from cd in caveData select cd.Clone()).ToArray()
                }).OrderBy(c => c.Name).ToArray();

            return response;
        }

        //public API.DataTemplateAddUpdateResponse DataTemplateAddUpdate(API.DataTemplateAddUpdateRequest request)
        //{
        //    var session = FindSession(request.SessionId);
        //    if (session == null)
        //        return Fail<API.DataTemplateAddUpdateResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

        //    var user = session.User;

        //    var response = new API.DataTemplateAddUpdateResponse();
        //    response.CopySessionInfo(request);

        //    DataTemplate template = null;
        //    // check to see if it is an update
        //    if (request.Id.HasValue)
        //    {
        //        template = _db.Table<DataTemplate>().FirstOrDefault(t => t.Id == request.Id.Value);
        //        if (null == template)
        //            return Fail<API.DataTemplateAddUpdateResponse>(request, HttpStatusCode.BadRequest, "The specified Id was invalid.");
        //    }
        //    else
        //        template = new DataTemplate();

        //    template.Name = request.Name;
        //    template.Description = request.Description;
        //    if (request.Items != null && request.Items.Length > 0)
        //        template.Items = request.Items
        //            .Select(t => new DataTemplateItem() { Name = t.Name, Description = t.Description, Type = t.Type, MetaData = t.MetaData, DefaultValue = t.DefaultValue })
        //            .ToList();

        //    _db.Save(template);

        //    response.Id = template.Id;
        //    response.Status = (int)HttpStatusCode.OK;

        //    return response;
        //}

        //public API.DataTemplateGetResponse DataTemplateGet(API.DataTemplateGetRequest request)
        //{
        //    var session = FindSession(request.SessionId);
        //    if (session == null)
        //        return Fail<API.DataTemplateGetResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

        //    var response = new API.DataTemplateGetResponse();
        //    response.CopySessionInfo(request);

        //    var apiTemplates = new List<DataTemplate>();
        //    if (request.TemplateIds != null && request.TemplateIds.Length > 0)
        //    {
        //        apiTemplates.AddRange(
        //            _db.Table<DataTemplate>()
        //            .Where(dt => request.TemplateIds.Contains(dt.Id))
        //            .Select(dt => dt.Clone()));
        //    }
        //    else
        //    {
        //        apiTemplates.AddRange(
        //            _db.Table<DataTemplate>()
        //            .Select(dt => dt.Clone()));
        //    }

        //    response.Templates = apiTemplates.ToArray();
        //    response.Status = (int)HttpStatusCode.OK;
        //    return response;
        //}

        public object GenericInvokeCommand(object request)
        {
            Type requestType = request.GetType();
            Func<object, object> exec = null;
            if (_commands.TryGetValue(requestType, out exec))
            {
                return exec(request);
            }
            else
                return null;
        }
    }
}
