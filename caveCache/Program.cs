using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace caveCache
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0 || args[0] == "-i")
                {
                    // initialize the database
                    var config = new ConfigurationReader();
                    var db = new Database.CaveCacheContext(config.ConnectionString);
                    var loggerFactory = db.GetService<ILoggerFactory>();
                    loggerFactory.AddProvider(new ConsoleLoggerProvider());
                    var mediaCache = new MediaCache(config);
                    

                    CommandRunner cmd = new CommandRunner(config, mediaCache, db, true);

                    if (args[0] == "-i")
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
                    var config = new ConfigurationReader();
                    var db = new Database.CaveCacheContext(config.ConnectionString);
                    var loggerFactory = db.GetService<ILoggerFactory>();
                    loggerFactory.AddProvider(new ConsoleLoggerProvider());
                    var mediaCache = new MediaCache(config);

                    using (var api = new CaveCacheHttp( config, mediaCache, db ))
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

        private static void RunCommand(string[] args, CommandRunner cmd)
        {
            string sessionId = cmd.GetAdminSession();
            Console.WriteLine("sessionId = {0}", sessionId);
            object response = "Not Implemented";
            switch (args[0])
            {
                // admin commands
                case "-bootstrap":
                    cmd.BootStrap();
                    break;
                case "-listusers":
                    response = cmd.UserList(new API.UserListRequest() { RequestId = 0, SessionId = sessionId });
                    break;
                case "-adduser":
                    {
                        if (args.Length == 4 || args.Length == 3)
                        {
                            string password = (args.Length == 3) ? string.Empty : args[3];
                            response = cmd.AddUser(new API.UserAddRequest() { Email = args[1], Name = args[2], Password = password });
                        }
                        else
                            response = "Invalid command: -adduser <email> <name> [<password>]";
                    }
                    break;
                case "-listsessions":
                    response = cmd.SessionList(new API.SessionListRequest());
                    break;
                case "-resetuserpassword":
                    if (args.Length != 2)
                    {
                        response = "Invalid Command: -resetuserpassword <email>";
                    }
                    else
                        response = cmd.ResetPassword(new API.UserResetPasswordRequest() { Email = args[1] });
                    break;
                case "-listallcaves":
                    response = cmd.CaveList(new API.CaveListRequest()
                    {
                        SessionId = sessionId,
                        allCaves = true
                    });
                    break;
                case "-restorecave":
                    break;
                case "-listallsurveys":
                    break;

                // normal user commands
                case "-login":
                    {
                        string email = "admin";
                        string password;

                        if (args.Length == 3)
                        {
                            email = args[1];
                            password = args[2];
                        }
                        else
                            password = args[1];

                        var login = new API.LoginRequest()
                        {
                            Email = email,
                            Password = password,
                            RequestId = 0
                        };
                        response = cmd.Login(login);
                    }
                    break;
                case "-addsurvey":
                    break;
                case "-addcave":
                    if (args.Length < 7)
                        response = "Invalid Command: -addcave <Name> <Description> <Latitude> <Longitude> <Accuracy> <Altitude>";
                    else
                        response = cmd.CaveAddUpdate(new API.CaveAddUpdateRequest()
                        {
                            SessionId = sessionId,
                            Name = args[1],
                            Description = args[2],
                            LocationId = 1,
                            Locations = new Database.CaveLocation[]{
                                new Database.CaveLocation{
                                    LocationId = 1,
                                    Latitude = decimal.Parse(args[3]),
                                    Longitude = decimal.Parse(args[4]),
                                    Accuracy = int.Parse(args[5]),
                                    Altitude = int.Parse(args[6])
                                }
                            }
                        });
                    break;
                case "-removecave":
                    break;
                case "-sharecave":
                    break;
                case "-listcaves":
                    response = cmd.CaveList(new API.CaveListRequest() { allCaves = false, SessionId = sessionId });
                    break;
                case "-addsurveyuser":
                    break;
                case "-modsuveyuser":
                    break;
                case "-delsurveyuser":
                    break;
                case "-addsurveycave":
                    break;
                case "-removesurveycave":
                    break;
                case "-usergetinfo":
                    response = cmd.UserGetInfo(new API.UserGetInfoRequest() { SessionId = sessionId });
                    break;
            }

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented));

        }
    }

}
