using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaveCache.Nouns.Http
{
  public class UserUpdateRequest
  {
    public string Id;
    public string Name;
    public string Email;
    public string Profile;
    public string Permissions;
  }
}
