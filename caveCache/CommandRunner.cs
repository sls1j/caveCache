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
using caveCache.API;

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

        public void Cleanup()
        {
            DateTime now = DateTime.Now;
            // clean up unsaved caves older than 1 day
            var uncleanCaves = _db.Caves.Where(c => (!c.Saved) && (now - c.CreatedDate).TotalDays >= 1.0).ToArray();
            var caveIds = new HashSet<int>(uncleanCaves.Select(c => c.CaveId));
            _db.CaveData.RemoveRange(_db.CaveData.Where(cd => caveIds.Contains(cd.CaveId)));
            _db.CaveLocation.RemoveRange(_db.CaveLocation.Where(cl => caveIds.Contains(cl.CaveId)));
            _db.CaveNote.RemoveRange(_db.CaveNote.Where(cn => caveIds.Contains(cn.CaveId)));
            _db.CaveUsers.RemoveRange(_db.CaveUsers.Where(cu => caveIds.Contains(cu.CaveId)));
            _db.Caves.RemoveRange(uncleanCaves);
            _db.SaveChanges();
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
                return new API.LoginResponse() { RequestId = request.RequestId, Status = (int)HttpStatusCode.Unauthorized, StatusDescription = "Username or Password is incorrect" };
            }

            // verify password
            var hash = HashPassword(request.Password, user.PasswordSalt);
            if (hash != user.PasswordHash)
            {
                HistoryEntry(null, null, null, null, "Failed login for user {0}. Bad password.", user);
                return new API.LoginResponse() { RequestId = request.RequestId, Status = (int)HttpStatusCode.Unauthorized, StatusDescription = "Username or Password is incorrect" };
            }

            user.LastLoggedIn = DateTime.Now;

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

        public GetMediaResponse GetMediaStream(GetMediaRequest request)
        {
            var resp = new GetMediaResponse();

            var session = FindSession(request.SessionId);
            if (session != null)
            {
                if (CheckGetMediaPermission(session, request.MediaId))
                {
                    var stream = _cache.GetMediaDataStream(request.MediaId);
                    return new GetMediaResponse()
                    {
                        RequestId = request.RequestId,
                        SessionId = request.SessionId,
                        MediaId = request.MediaId,
                        Stream = stream,
                        StatusDescription = string.Empty,
                        Status = (int)HttpStatusCode.OK
                    };
                }
                else
                    return Fail<GetMediaResponse>(request, HttpStatusCode.BadRequest, "Do not have permissions to access requested media.");
            }
            else
                return Fail<GetMediaResponse>(request, HttpStatusCode.Forbidden, "Must login first.");
        }

        public SetMediaResponse SetMedia(SetMediaRequest request)
        {
            var session = FindSession(request.SessionId);
            if (session != null)
            {

                var media = new Media()
                {
                    FileName = request.FileName,
                    FileSize = request.FileSize,
                    MimeType = request.MimeType,
                    AttachId = request.AttachId,
                    AttachType = request.AttachType
                };

                if (request.FileSize > _config.MaxMediaSize)
                    return Fail<SetMediaResponse>(request, HttpStatusCode.BadRequest, "Media too large.");

                _db.Media.Add(media);
                _db.SaveChanges();

                if (_cache.SetMediaDataStream(media.MediaId, request.Media))
                {

                    var resp = new SetMediaResponse()
                    {
                        MediaId = media.MediaId,
                        RequestId = request.RequestId,
                        SessionId = request.SessionId,
                        StatusDescription = string.Empty,
                        Status = (int)HttpStatusCode.OK
                    };

                    return resp;
                }
                else
                {
                    // remove the record that failed to save.
                    _db.Media.Remove(media);
                    _db.SaveChanges();

                    return Fail<SetMediaResponse>(request, HttpStatusCode.InternalServerError, "Failed to save the media. Disk IO error.");
                }
            }
            else
                return Fail<SetMediaResponse>(request, HttpStatusCode.Forbidden, "Must login first.");
        }

        private bool CheckGetMediaPermission(UserSession session, int mediaId)
        {
            // via cave media
            return true;

            // via survey media            

            // via survey cave media


            // have permission if the 
            //return false;
        }

        private T Fail<T>(API.SessionRequest request, HttpStatusCode code, string errorMessage) where T : API.SessionResponse, new()
        {
            var response = new T()
            {
                RequestId = request.RequestId,
                SessionId = request.SessionId,
                Status = (int)code,
                StatusDescription = errorMessage
            };

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

            _db.Users.Add(user);
            _db.SaveChanges();

            _db.History.Add(HistoryEntry(session.UserId, null, null, null, "{0} Created user {1}", session.User, user));
            _db.SaveChanges();


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
                    select Combine(s, u);

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
            return new API.UserResetPasswordResponse()
            {
                Status = (int)HttpStatusCode.NotImplemented,
                RequestId = request.RequestId,
            };
        }

        public API.CaveCreateResponse CaveCreate(API.CaveCreateRequest request)
        {
            var session = FindSession(request.SessionId);
            if (session == null)
                return Fail<API.CaveCreateResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

            int nameId = 1;
            if (_db.Caves.Count() > 0)
                nameId = _db.Caves.Max(c => c.CaveId) + 1;

            Cave cave = new Cave() { Name = $"CC #{nameId}", Description = string.Empty, CreatedDate = DateTime.Now, Notes = string.Empty };
            _db.Caves.Add(cave);
            _db.SaveChanges();

            CaveUser caveUser = new CaveUser() { UserId = session.UserId, CaveId = cave.CaveId };
            _db.CaveUsers.Add(caveUser);
            _db.History.Add(HistoryEntry(session.UserId, cave.CaveId, null, null, $"Created new cave {cave.CaveId}"));
            _db.SaveChanges();

            var ci = GetCaveInfo(cu => cu.Cave.CaveId == cave.CaveId).First();

            return new CaveCreateResponse()
            {
                Cave = ci.Cave,
                SessionId = request.SessionId,
                RequestId = request.RequestId,
                Status = (int)HttpStatusCode.OK
            };
        }

        public API.CaveUpdateResponse CaveAddUpdate(API.CaveUpdateRequest request)
        {
            var session = FindSession(request.SessionId);
            if (session == null)
                return Fail<API.CaveUpdateResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

            if (string.IsNullOrEmpty(request.Name))
                return Fail<API.CaveUpdateResponse>(request, HttpStatusCode.BadRequest, "Cave must have a name.");

            Cave cave = null;
            cave = _db.Caves.FirstOrDefault(c => c.CaveId == request.CaveId);
            if (null == cave)
                return Fail<API.CaveUpdateResponse>(request, HttpStatusCode.BadRequest, "Cannot update a non-existant cave.");

            cave.LocationId = null;
            _db.CaveLocation.RemoveRange(_db.CaveLocation.Where(cl => cl.CaveId == cave.CaveId).ToArray());
            _db.CaveNote.RemoveRange(_db.CaveNote.Where(cn => cn.CaveId == cave.CaveId).ToArray());
            _db.CaveData.RemoveRange(_db.CaveData.Where(cd => cd.CaveId == cave.CaveId).ToArray());
            _db.SaveChanges();

            cave.Name = request.Name;
            cave.Description = request.Description ?? string.Empty;
            cave.IsDeleted = false;

            if (request.Data != null)
            {
                _db.CaveData.AddRange(request.Data.Select(d => new CaveData() { CaveId = cave.CaveId, Name = d.Name, Type = d.Type, MetaData = d.MetaData ?? string.Empty, Value = d.Value }));
            }

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
                    Notes = l.Notes ?? string.Empty,
                    Source = l.Source ?? string.Empty,
                    Unit = l.Unit ?? "Emperial",
                };

                _db.CaveLocation.Add(location);
            }

            foreach (var n in request.Notes)
            {
                var cn = n.Clone();
                _db.CaveNote.Add(cn);
            }

            _db.History.Add(HistoryEntry(session.UserId, cave.CaveId, null, null, "Cave updated by {0}", session.User.Name));

            _db.SaveChanges();

            // get list of all associated media
            var deadMedia = new HashSet<Media>();
            foreach (var caveMedia in _db.Media.Where(m => m.AttachId == cave.CaveId && m.AttachType == "cave"))
            {
                bool notFound = true;
                var reference = $"src=\"/Media/{caveMedia.MediaId}\"";
                // check to see if it's contained in one of the notes
                foreach (var n in request.Notes)
                {
                    if (n.Notes.Contains(reference))
                    {
                        notFound = false;
                        break;
                    }
                }

                if (notFound)
                    deadMedia.Add(caveMedia);
            }

            if (deadMedia.Count > 0)
            {
                // remove the files
                foreach (var m in deadMedia)
                    _cache.RemoveMedia(m.MediaId);

                // remove the database entries
                _db.Media.RemoveRange(deadMedia);
                _db.SaveChanges();
            }

            return new API.CaveUpdateResponse()
            {
                RequestId = request.RequestId,
                SessionId = request.SessionId,
                CaveId = cave.CaveId,
                Status = (int)HttpStatusCode.OK
            };
        }

        public CaveRemoveResponse CaveRemove(CaveRemoveRequest request)
        {
            var session = FindSession(request.SessionId);
            if (session == null)
                return Fail<API.CaveRemoveResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

            var caveUser = _db.CaveUsers.FirstOrDefault(cu => cu.UserId == session.UserId && cu.CaveId == request.CaveId);
            if (caveUser == null)
                return Fail<API.CaveRemoveResponse>(request, HttpStatusCode.NotFound, $"Cave with id {request.CaveId} not found.");

            var userCount = _db.Caves.Where(cu => cu.CaveId == request.CaveId).Count();

            if (userCount == 1)
            {
                var cave = _db.Caves.FirstOrDefault(c => c.CaveId == request.CaveId);
                _db.Caves.Remove(cave);
                _db.History.Add(HistoryEntry(session.UserId, request.CaveId, null, null, "Cave deleted."));
            }
            else
            {
                _db.CaveUsers.Remove(caveUser);
                _db.History.Add(HistoryEntry(session.UserId, request.CaveId, null, null, "Cave user deleted."));
            }

            _db.SaveChanges();

            return new API.CaveRemoveResponse() { RequestId = request.RequestId, SessionId = request.SessionId, Status = (int)HttpStatusCode.OK, StatusDescription = "OK" };
        }

        private class CaveTuple
        {
            public int UserId;
            public CaveInfo Cave;

            public CaveTuple(int userId, CaveInfo cave)
            {
                this.UserId = userId;
                this.Cave = cave;
            }
        }

        private T[] SafeToArray<T>(IEnumerable<T> list)
        {
            if (list == null)
                return new T[0];
            else
                return list.ToArray();
        }

        private IEnumerable<CaveTuple> GetCaveInfo(Func<CaveUser, bool> whereClause = null)
        {
            IEnumerable<CaveUser> caveUsers = _db.CaveUsers.Include(t => t.Cave);
            if (null != whereClause)
                caveUsers = caveUsers.Where(whereClause);

            return
                from cu in caveUsers
                join cl in _db.CaveLocation on cu.CaveId equals cl.CaveId into caveLocation
                join cd in _db.CaveData on cu.CaveId equals cd.CaveId into caveData
                join cn in _db.CaveNote on cu.CaveId equals cn.CaveId into caveNotes
                select new CaveTuple(cu.UserId, new API.CaveInfo()
                {
                    CaveId = cu.CaveId,
                    LocationId = cu.Cave.LocationId,
                    Locations = SafeToArray(from cl in caveLocation select cl.Clone()),
                    Description = cu.Cave.Description,
                    Name = cu.Cave.Name,
                    Notes = SafeToArray(from cn in caveNotes select cn.Clone()),
                    CaveData = SafeToArray(from cd in caveData select cd.Clone()),
                });
        }

        public API.CaveListResponse CaveList(API.CaveListRequest request)
        {
            var session = FindSession(request.SessionId);
            if (session == null)
                return Fail<API.CaveListResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

            API.CaveListResponse response = new API.CaveListResponse() { RequestId = request.RequestId, SessionId = request.SessionId, Status = (int)HttpStatusCode.OK };
            var caveQuery = _db.Caves.Include(t => t.Data).Include(t => t.Locations);

            if (session.User.Permissions.Contains("admin") && request.allCaves)
                response.Caves = GetCaveInfo().Select(c => c.Cave).OrderBy(c => c.Name).ToArray();
            else
                response.Caves = GetCaveInfo().Where(c => c.UserId == session.UserId).Select(c => c.Cave).OrderBy(c => c.Name).ToArray();


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
                UserId = user.UserId,
                Name = user.Name,
                Profile = user.Profile,
                Permissions = user.Permissions,
                Status = (int)HttpStatusCode.OK,
                Data = user.Data.Select(d => d.Clone()).ToArray()
            };

            // get cave data
            response.Caves = GetCaveInfo().Where(c => c.UserId == user.UserId).Select(c => c.Cave).OrderBy(c => c.Name).ToArray();

            return response;
        }

        public API.CleanMediaResponse CleanMedia(API.CleanMediaRequest request)
        {
            var session = FindSession(request.SessionId);
            if (session != null)
            {
                var deadMedia = new List<Media>();

                foreach (var m in _db.Media.Where(m2 => m2.AttachType == "cave" || m2.AttachType == "0"))
                {
                    string src = $"src=\"/Media/{m.MediaId}\"";
                    if (!_db.CaveNote.Any(cn => cn.Notes.Contains(src)))
                        deadMedia.Add(m);
                }

                // remove the dead media
                foreach (var m in deadMedia)
                {
                    Console.WriteLine($"Deleting Media {m.MediaId}");
                    _cache.RemoveMedia(m.MediaId);
                    _db.Remove(m);
                }

                _db.SaveChanges();

                return new CleanMediaResponse()
                {
                    RequestId = request.RequestId,
                    SessionId = request.SessionId,
                    Status = (int)HttpStatusCode.OK,
                    StatusDescription = "OK"
                };
            }
            else
                return Fail<CleanMediaResponse>(request, HttpStatusCode.Forbidden, "Must login first.");
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
            if (_commands.TryGetValue(requestType, out Func<object, object> exec))
            {
                return exec(request);
            }
            else
                return null;
        }
    }
}
