using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaveCache.Nouns.Http
{
    public class UserNewRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }        
        public string Profile { get; set; }
        public string Permissions { get; set; }
        public string Password { get; set; }
    }
}
