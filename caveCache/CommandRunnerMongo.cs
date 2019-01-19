using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using caveCache.API;
using MongoDB.Bson;
using caveCache.MongoDb;
using MongoDB.Driver.Linq;
using MongoDB.Driver;

namespace caveCache
{
  class CommandRunnerMongo
  {
    private MongoDb.CaveContext _db;
    private RandomNumberGenerator _rng;
    private IConfiguration _config;
    private IMediaCache _cache;
    private bool _isCommandLine;
    private Dictionary<Type, Func<object, object>> _commands;

    private readonly ObjectId AdminObjectId = new ObjectId(new byte[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

    public CommandRunnerMongo(IConfiguration config, IMediaCache cache, MongoDb.CaveContext db, bool isCommandLine = false)
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
      //DateTime now = DateTime.Now;
      //// clean up unsaved caves older than 1 day
      //var uncleanCaves = _db.Caves.Where(c => (!c.Saved) && (now - c.CreatedDate).TotalDays >= 1.0).ToArray();
      //var caveIds = new HashSet<int>(uncleanCaves.Select(c => c.CaveId));
      //_db.CaveData.RemoveRange(_db.CaveData.Where(cd => caveIds.Contains(cd.CaveId)));
      //_db.CaveLocation.RemoveRange(_db.CaveLocation.Where(cl => caveIds.Contains(cl.CaveId)));
      //_db.CaveNote.RemoveRange(_db.CaveNote.Where(cn => caveIds.Contains(cn.CaveId)));
      //_db.CaveUsers.RemoveRange(_db.CaveUsers.Where(cu => caveIds.Contains(cu.CaveId)));
      //_db.Caves.RemoveRange(uncleanCaves);
      //_db.SaveChanges();
    }

    public void BootStrap()
    {
      // check that the boot strap hasn't already run           
      if (_db.IsBootStrapped)
      {
        Console.WriteLine("Already boot strapped");
        return;
      }

      // create admin user
      User user = new User()
      {
        Id = AdminObjectId,
        Created = DateTime.Now,
        Email = "admin",
        Expire = null,
        Name = "Admin",
        Permissions = "admin",
        Profile = ""
      };

      string password = "password";
      GenerateUserPasswordHash(user, password);

      _db.Users.InsertOne(user);
      _db.IsBootStrapped = true;

      Console.WriteLine("System is ready for use.");
    }

    private History HistoryEntry(ObjectId? UserId, ObjectId? CaveId, ObjectId? SurveyId, ObjectId? MediaId, string Description, params object[] args)
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
        Data = new BsonDocument()
      };


      return history;
    }

    public string GetAdminSession()
    {
      var session = _db.UserSessions.AsQueryable().FirstOrDefault(s => s.Id == AdminObjectId);
      if (session != null)
        return session.SessionId;
      else
        return "";
    }

    public API.LoginResponse Login(API.LoginRequest request)
    {
      // find user
      var user = _db.Users.AsQueryable().FirstOrDefault(u => u.Email == request.Email);
      if (null == user)
      {
        HistoryEntry(null, null, null, null, "Failed login for bad user {0}", request.Email);
        return new API.LoginResponse() { RequestId = request.RequestId, Status = (int)HttpStatusCode.Unauthorized, StatusDescription = "Username or Password is incorrect" };
      }

      // only allow the admin password via the command line
      if (user.Email == "admin" && !_isCommandLine)
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
        UserId = user.Id
      };

      var deadSessions = _db.UserSessions.DeleteMany(s => s.Timeout < now);
      _db.UserSessions.InsertOne(session);
      _db.History.InsertOne(HistoryEntry(user.Id, null, null, null, "User {0} logged in.", user));

      return new API.LoginResponse()
      {
        RequestId = request.RequestId,
        SessionId = session.SessionId,
        UserId = user.Id,
        Name = user.Name,
        Profile = user.Profile,
        Permissions = user.Permissions,
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

        _db.Media.InsertOne(media);

        if (_cache.SetMediaDataStream(media.Id, request.Media))
        {

          var resp = new SetMediaResponse()
          {
            MediaId = media.Id,
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
          _db.Media.DeleteOne(m => m.Id == media.Id);

          return Fail<SetMediaResponse>(request, HttpStatusCode.InternalServerError, "Failed to save the media. Disk IO error.");
        }
      }
      else
        return Fail<SetMediaResponse>(request, HttpStatusCode.Forbidden, "Must login first.");
    }

    private bool CheckGetMediaPermission(UserSession session, ObjectId mediaId)
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

    private T Succes<T>(API.SessionRequest request) where T : API.SessionResponse, new()
    {
      var response = new T()
      {
        RequestId = request.RequestId,
        SessionId = request.SessionId,
        Status = (int)HttpStatusCode.OK
      };

      return response;
    }


    public API.UserAddResponse AddUser(API.UserAddRequest request)
    {
      // look for session
      var session = FindSession(request.SessionId);
      if (null == session || !session.User.Permissions.Contains("admin"))
      {
        HistoryEntry(session.User.Id, null, null, null, "{0} Failed to create user {1}.  Not authorized", session.User, request.Email);
        return Fail<API.UserAddResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");
      }

      // check that there isn't already a user      
      if (_db.Users.AsQueryable().Any(u => string.Equals(u.Email, request.Email, StringComparison.CurrentCultureIgnoreCase)))
      {
        _db.History.InsertOne(HistoryEntry(session.User.Id, null, null, null, "{0}Failed to create user with email {0}.  Already exists", session.User.Name, request.Email));
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

      _db.Users.InsertOne(user);

      _db.History.InsertOne(HistoryEntry(session.UserId, null, null, null, "{0} Created user {1}", session.User, user));

      // return user object
      return new API.UserAddResponse()
      {
        UserId = user.Id.ToString(),
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

    private UserSession FindSession(string sessionId)
    {

      UserSession session = null;

      bool isExpired = false;
      do
      {
        var sessionQuery = _db.UserSessions.AsQueryable();
        if (_isCommandLine)
          session = sessionQuery.FirstOrDefault(s => s.IsCommandLine);
        else
          session = sessionQuery.FirstOrDefault(s => s.SessionId == sessionId);


        if (null != session && session.Timeout < DateTime.UtcNow)
        {
          _db.UserSessions.DeleteOne(s => s.Id == session.Id);
          isExpired = true;
        }
        else
          isExpired = false;
      }
      while (isExpired);

      if (session != null)
      {
        session.Timeout = DateTime.UtcNow.AddHours(1);
      }

      session.User = _db.Users.Find(u => u.Id == session.UserId).First();

      return session;
    }

    public API.UserListResponse UserList(API.UserListRequest request)
    {
      var session = FindSession(request.SessionId);
      if (session == null || !session.User.Permissions.Contains("admin"))
        return Fail<API.UserListResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

      var response = new API.UserListResponse() { RequestId = request.RequestId, SessionId = request.SessionId, Status = (int)HttpStatusCode.OK };

      response.Users = _db.Users.AsQueryable().ToArray();
      return response;
    }

    public API.SessionListResponse SessionList(API.SessionListRequest request)
    {
      var session = FindSession(request.SessionId);
      if (session == null || !session.User.Permissions.Contains("admin"))
        return Fail<API.SessionListResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

      var response = new API.SessionListResponse() { RequestId = request.RequestId, SessionId = request.SessionId, Status = (int)HttpStatusCode.OK };

      response.Sessions = _db.UserSessions.AsQueryable().ToArray();
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

      int nameId = _db.GetNextCaveNumber();

      Cave cave = new Cave() { Name = $"CC #{nameId}", Description = string.Empty, CreatedDate = DateTime.Now };
      _db.Caves.InsertOne(cave);
      _db.Users.UpdateOne(u => u.Id == session.UserId, Builders<User>.Update.AddToSet(u => u.Caves, cave.Id));
      _db.History.InsertOne(HistoryEntry(session.UserId, cave.Id, null, null, $"Created new cave {cave.Name}:{cave.Id}"));      

      return new CaveCreateResponse()
      {
        Cave = new CaveInfo(cave),
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
      cave = _db.Caves.Find(c => c.Id == request.CaveId).Single();
      if (null == cave)
        return Fail<API.CaveUpdateResponse>(request, HttpStatusCode.BadRequest, "Cannot update a non-existant cave.");

      cave.Name = request.Name;
      cave.Description = request.Description ?? string.Empty;
      cave.IsDeleted = false;

      if (request.Data != null)
      {
        cave.Data = request.Data.ToList();
      }

      if (request.Locations != null)
      {
        cave.Locations = request.Locations.ToList();
      }

      if (request.Notes != null)
      {
        cave.Notes = request.Notes.ToList();
      }

      _db.History.InsertOne(HistoryEntry(session.UserId, cave.Id, null, null, $"Cave {cave.Id} updated by {session.User.Name}"));

      // get list of all associated media
      var deadMedia = new HashSet<ObjectId>();
      foreach (var caveMedia in _db.Media.AsQueryable().Where(m => m.AttachId == cave.Id && m.AttachType == "cave"))
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
          deadMedia.Add(caveMedia.Id);
      }

      if (deadMedia.Count > 0)
      {
        // remove the files
        foreach (var m in deadMedia)
          _cache.RemoveMedia(m);

        // remove the database entries
        _db.Media.DeleteMany(m => deadMedia.Contains(m.Id));
      }

      return new API.CaveUpdateResponse()
      {
        RequestId = request.RequestId,
        SessionId = request.SessionId,
        CaveId = cave.Id,
        Status = (int)HttpStatusCode.OK
      };
    }

    public CaveRemoveResponse CaveRemove(CaveRemoveRequest request)
    {
      var session = FindSession(request.SessionId);
      if (session == null)
        return Fail<API.CaveRemoveResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

      if (!session.User.Caves.Contains(request.CaveId))
        return Fail<API.CaveRemoveResponse>(request, HttpStatusCode.NotFound, $"Cave with id {request.CaveId} not found.");

      var userCount = _db.Users.Find(u => u.Caves.Contains(request.CaveId)).CountDocuments();

      if (userCount == 1)
      {
        var cave = _db.Caves.DeleteOne(c => c.Id == request.CaveId);
        _db.History.InsertOne(HistoryEntry(session.UserId, request.CaveId, null, null, $"Cave {request.CaveId} deleted by {session.User.Name}."));
      }

      session.User.Caves.Remove(request.CaveId);
      _db.Users.UpdateOne(u => u.Id == session.UserId, Builders<User>.Update.Set(u => u.Caves, session.User.Caves));
      _db.History.InsertOne(HistoryEntry(session.UserId, request.CaveId, null, null, $"{session.User.Name} no longer has access to cave {request.CaveId}"));

      return new API.CaveRemoveResponse() { RequestId = request.RequestId, SessionId = request.SessionId, Status = (int)HttpStatusCode.OK, StatusDescription = "OK" };
    }


    private T[] SafeToArray<T>(IEnumerable<T> list)
    {
      if (list == null)
        return new T[0];
      else
        return list.ToArray();
    }

    public API.CaveListResponse CaveList(API.CaveListRequest request)
    {
      var session = FindSession(request.SessionId);
      if (session == null)
        return Fail<API.CaveListResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

      API.CaveListResponse response = new API.CaveListResponse() { RequestId = request.RequestId, SessionId = request.SessionId, Status = (int)HttpStatusCode.OK };

      if (session.User.Permissions.Contains("admin") && request.allCaves)
        response.Caves = GetCaveInfo("").OrderBy(c => c.Name).ToArray();
      else
        response.Caves = GetCaveInfo(Builders<Cave>.Filter.In( c => c.Id,  session.User.Caves)).OrderBy(c => c.Name).ToArray();

      return response;
    }

    private IEnumerable<CaveInfo> GetCaveInfo(FilterDefinition<Cave> filter)
    {
      return _db.Caves.Find(filter)
        .ToList()
        .Select(c => new CaveInfo(c));
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
        UserId = user.Id,
        Name = user.Name,
        Profile = user.Profile,
        Permissions = user.Permissions,
        Status = (int)HttpStatusCode.OK,
        Data = user.Data.Select(d => d.Clone()).ToArray()
      };

      // get cave data
      response.Caves = GetCaveInfo(Builders<Cave>.Filter.In( c => c.Id, user.Caves)).OrderBy(c => c.Name).ToArray();

      return response;
    }

    public API.UserGetProfileResponse UserGetProfile(API.UserGetProfileRequest request)
    {
      var session = FindSession(request.SessionId);
      if (session == null)
        return Fail<API.UserGetProfileResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

      var user = session.User;

      var response = Succes<API.UserGetProfileResponse>(request);
      response.Email = user.Email;
      response.Name = user.Name;
      response.Permissions = user.Permissions;
      response.Profile = user.Profile;

      return response;
    }

    public API.UserSetProfileResponse UserSetProfile(API.UserSetProfileRequest request)
    {
      var session = FindSession(request.SessionId);
      if (session == null)
        return Fail<API.UserSetProfileResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

      var user = session.User;
      if (!string.IsNullOrWhiteSpace(request.Email))
        user.Email = request.Email;
      if (!string.IsNullOrWhiteSpace(request.Name))
        user.Name = request.Name;
      user.Profile = request.Profile;

      _db.Users.ReplaceOne(u => u.Id == user.Id, user);

      return Succes<API.UserSetProfileResponse>(request);
    }

    public API.UserSetPasswordResponse UserSetPassword(API.UserSetPasswordRequest request)
    {
      var session = FindSession(request.SessionId);
      if (session == null)
        return Fail<API.UserSetPasswordResponse>(request, HttpStatusCode.Unauthorized, "Unauthorized");

      var hash = HashPassword(request.OldPassword, session.User.PasswordSalt);
      if (hash != session.User.PasswordHash)
        return Fail<API.UserSetPasswordResponse>(request, HttpStatusCode.Unauthorized, "Old password didn't match.");

      GenerateUserPasswordHash(session.User, request.NewPassword);
      _db.Users.ReplaceOne(u => u.Id == session.UserId, session.User);

      return Succes<API.UserSetPasswordResponse>(request);
    }

    public API.CleanMediaResponse CleanMedia(API.CleanMediaRequest request)
    {
      var session = FindSession(request.SessionId);
      if (session != null)
      {
        var deadMedia = new List<ObjectId>();

        foreach (var m in _db.Media.AsQueryable().Where(m2 => m2.AttachType == "cave" || m2.AttachType == "0"))
        {
          string src = $"src=\"/Media/{m.Id}\"";
          if (!_db.Caves.AsQueryable().Any(c => c.Notes.Any( cn => cn.Note.Contains(src))))
            deadMedia.Add(m.Id);
        }

        // remove the dead media
        foreach (var m in deadMedia)
        {
          Console.WriteLine($"Deleting Media {m}");
          _cache.RemoveMedia(m);          
        }

        _db.Media.DeleteMany(m => deadMedia.Contains(m.Id));

        return Succes<CleanMediaResponse>(request);
      }
      else
        return Fail<CleanMediaResponse>(request, HttpStatusCode.Forbidden, "Must login first.");
    }

    public API.CaveShareResponse ShareCave(API.CaveShareRequest request)
    {
      var session = FindSession(request.SessionId);
      if (session != null)
      {
        // make sure the user can share the cave                
        if (session.User.Caves.Contains(request.CaveId))
        {
          if (_db.Users.CountDocuments(u => u.Id == request.UserId) == 1 )
          {
            _db.Users.UpdateOne(u => u.Id == request.UserId, Builders<User>.Update.AddToSet(u => u.Caves, request.CaveId));

            return Succes<CaveShareResponse>(request);
          }
          else
            return Fail<CaveShareResponse>(request, HttpStatusCode.BadRequest, "Unknown user id.");
        }
        else
          return Fail<CaveShareResponse>(request, HttpStatusCode.BadRequest, "Unknown cave id.");
      }
      else
        return Fail<CaveShareResponse>(request, HttpStatusCode.Forbidden, "Must login first.");
    }

    public API.UserGetShareListResponse UserGetShareList(API.UserGetShareListRequest request)
    {
      var session = FindSession(request.SessionId);
      if (session != null)
      {
        // make sure the cave exists
        if (_db.Caves.CountDocuments(c => c.Id == request.CaveId) > 0)
        {
          var users = _db.Users.Find( u => !u.Caves.Contains(request.CaveId)).ToList().Select(u => new UserShort(u.Id, u.Name, u.Email)).ToList();
          
          var response = Succes<UserGetShareListResponse>(request);
          response.Users = users.ToArray();
          return response;
        }
        else
          return Fail<UserGetShareListResponse>(request, HttpStatusCode.BadRequest, "Unknown cave.");
      }
      else
        return Fail<UserGetShareListResponse>(request, HttpStatusCode.Forbidden, "Must login first.");
    }  

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
