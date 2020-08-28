using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaveCache.Nouns.Http
{
  public class LoginResponse : Response
  {
    public string SessionId;
    public string UserId;
    public string Name;
    public string Profile;
    public string Permissions;
  }
}
