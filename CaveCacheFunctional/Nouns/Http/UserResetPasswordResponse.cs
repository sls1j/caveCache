using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaveCache.Nouns.Http
{
  public class UserResetPasswordResponse : Response
  {
    public string NewPassword { get; set; }
  }
}
