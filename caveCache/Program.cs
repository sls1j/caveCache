using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace caveCache
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      var p = new Program();
      p.ProgMain(args);
    }

    private ConfigurationReader _config;
    private MongoDb.CaveContext _db;
    private MediaCache _mediaCache;

    private void ProgMain(string[] args)
    {
      _config = new ConfigurationReader();
      _db = new MongoDb.CaveContext(_config.ConnectionString);
      _mediaCache = new MediaCache(_config);
      //var loggerFactory = db.GetService<ILoggerFactory>();
      //loggerFactory.AddProvider(new ConsoleLoggerProvider());

      try
      {
        if (args.Length == 0 || args[0] == "-i")
        {
          // initialize the database

          CommandRunnerMongo cmd = new CommandRunnerMongo(_config, _mediaCache, _db, true);

          if (args.Length > 0 && args[0] == "-i")
          {
            Console.WriteLine("New Cave Survey Server {0} Interaction Mode", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            Console.Write("> ");
            var line = Console.ReadLine();
            while (line != "quit")
            {
              string[] cmdArgs = SplitLine(line);
              RunCommand(cmdArgs, cmd);
              Console.Write("> ");
              line = Console.ReadLine();
            }
          }

          RunCommand(args, cmd);
        }
        else if (args[0] == "-runhttpapi")
        {
          using (var api = new CaveCacheHttp(_config, _mediaCache, _db))
          {
            while (true)
              System.Threading.Thread.Sleep(250);
          }
        }
      }
      finally
      {
        if (System.Diagnostics.Debugger.IsAttached)
          Console.ReadKey();
      }
    }

    private static string[] SplitLine(string line)
    {
      var args = new List<string>();
      var sb = new StringBuilder();
      int state = 0;

      for (int i = 0; i < line.Length; i++)
      {
        char c = line[i];
        switch (state)
        {
          case 0:
            if (char.IsWhiteSpace(c))
              state = 0;
            else if (c == '"')
              state = 2;
            else
            {
              sb.Append(c);
              state = 1;
            }
            break;
          case 1:
            if (char.IsWhiteSpace(c))
              state = 4;
            else
            {
              state = 1;
              sb.Append(c);
            }
            break;
          case 2:
            if (c == '"')
              state = 4;
            else if (c == '\\')
              state = 3;
            else
            {
              state = 2;
              sb.Append(c);
            }
            break;
          case 3:
            sb.Append(c);
            state = 2;
            break;
          case 4:
            if (char.IsWhiteSpace(c))
              state = 4;
            else if (c == '"')
            {
              args.Add(sb.ToString());
              sb = new StringBuilder();
              state = 2;
            }
            else
            {
              args.Add(sb.ToString());
              sb = new StringBuilder();
              sb.Append(c);
              state = 1;
            }
            break;
        }
      }

      if (sb.Length > 0)
        args.Add(sb.ToString());

      return args.ToArray();
    }

    private Dictionary<string, Action<CommandArgs, CommandRunnerMongo>> _commands;

    private class CommandArgs
    {
      public string[] a;
      public object response;
      public bool quit;
    }

    private bool RunCommand(string[] args, CommandRunnerMongo cmd)
    {
      if (args.Length == 0)
        return false;

      string sessionId = cmd.GetAdminSession();
      Console.WriteLine("sessionId = {0}", sessionId);
      CommandArgs commandArgs = new CommandArgs()
      {
        a = args,
        response = "Not Implemented"
      };

      if (null == _commands)
      {
        _commands = new Dictionary<string, Action<CommandArgs, CommandRunnerMongo>>()
        {
          { "quit", (a,c) => {a.quit = true; Console.WriteLine("Quitting..."); } },
          { "bootstrap", (a,c) => c.BootStrap() },
          { "listusers", (a,c) =>
            {
              var users = cmd.UserList(new API.UserListRequest() { RequestId = 0, SessionId = sessionId });
              foreach (var u in users.Users)
              {
                Console.WriteLine($"{u.Name} {u.Email} {u.IsActive} {u.LastLoggedIn} ");
              }
            } },
          { "adduser", (a,c) =>
            {
              if (a.a.Length == 4 || a.a.Length == 3)
              {
                string password = (a.a.Length == 3) ? string.Empty : a.a[3];
                a.response = c.AddUser(new API.UserAddRequest() { Email = a.a[1], Name = a.a[2], Password = password, Profile = string.Empty });
              }
              else
                a.response = "Invalid command: adduser <email> <name> [<password>]";
            } },
          { "listsessions", (a,c) => a.response = c.SessionList(new API.SessionListRequest()) },
          { "resetuserpassword", (a,c) =>
            {
            if (a.a.Length != 2)
            {
              a.response = "Invalid Command: resetuserpassword <email>";
            }
            else
              a.response = c.ResetPassword(new API.UserResetPasswordRequest() { Email = a.a[1] });
            }
            },
          {"listallcaves", (a,c) =>
            {
              a.response = c.CaveList(new API.CaveListRequest(){SessionId = sessionId,allCaves = true})
                .Caves
                .Select( cv => new {cv.Number, cv.Name, cv.LocalString })
                .ToArray();
            }
          },
          { "login", (a,c) =>
            {
              string email = "admin";
              string password;

              if (a.a.Length == 3)
              {
                email = a.a[1];
                password = a.a[2];
              }
              else
                password = a.a[1];

              var login = new API.LoginRequest()
              {
                Email = email,
                Password = password,
                RequestId = 0,
              };
              a.response = cmd.Login(login);
            } },
          {"addcave", (a,c) =>
            {
            if (a.a.Length < 7)
              a.response = "Invalid Command: addcave <Name> <Description> <Latitude> <Longitude> <Accuracy> <Altitude>";
            else
              a.response = c.CaveAddUpdate(new API.CaveUpdateRequest()
              {
                SessionId = sessionId,
                Name = a.a[1],
                Description = a.a[2],
                LocationId = 1,
                Locations = new MongoDb.CaveLocation[]{
                                  new MongoDb.CaveLocation{
                                      Latitude = decimal.Parse(a.a[3]),
                                      Longitude = decimal.Parse(a.a[4]),
                                      Accuracy = int.Parse(a.a[5]),
                                      Altitude = int.Parse(a.a[6])
                                  }
                              }
              });
            }
          },
          {"removecave", (a,c) =>
            {
              if (a.a.Length < 2)
                a.response = "Invalid Command: removecave <cave id>";
              else
               a.response = c.CaveRemove(new API.CaveRemoveRequest() { CaveId = ObjectId.Parse(a.a[1]) });
            }
          },
          { "listcaves", (a,c) =>
            {
              a.response =
              c.CaveList(new API.CaveListRequest() { allCaves = false, SessionId = sessionId })
                .Caves
                .Select( cv => new {cv.Number, cv.Name, cv.LocalString })
                .ToArray();
            }
          },
          {"usergetinfo", (a,c) => a.response = c.UserGetInfo(new API.UserGetInfoRequest() { SessionId = sessionId }) },
          { "cleanmedia", (a,c) => a.response = c.CleanMedia(new API.CleanMediaRequest() { SessionId = sessionId }) },
          { "help", (a,c) =>
            {
              a.response = string.Join( ",", _commands.Keys);
            }
            },
          { "http", (a,c) => {
          using (var api = new CaveCacheHttp(_config, _mediaCache, _db))
          {
            while (true)
              System.Threading.Thread.Sleep(250);
          }
          }}
      };
      }

      if (_commands.TryGetValue(args[0], out var action))
      {
        try
        {
          action(commandArgs, cmd);

          if (commandArgs.quit)
          {
            return true;
          }
          DumpJson(commandArgs.response);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
        }
      }
      else
      {
        Console.WriteLine($"No command {args[0]}");
      }

      return false;
    }

    private static void DumpJson(object o)
    {
      Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented));
    }
  }
}
