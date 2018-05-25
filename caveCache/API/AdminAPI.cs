using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.API
{
    class UserAddRequest : SessionRequest
    {
        public string Name;
        public string Email;
        public string Password;
        public string Profile;
    }

    class UserAddResponse : SessionResponse
    {
        public string Password;
    }

    class UserListRequest : SessionRequest
    {
    }

    class UserListResponse : SessionResponse
    {
        public Database.User[] Users;
    }
    
    class UserResetPasswordResponse : SessionResponse
    {
        public string NewPassword;
    }

    class SessionListRequest : SessionRequest { }
    class SessionListResponse : SessionResponse
    {
        public Database.UserSession[] Sessions;
    }    
}
