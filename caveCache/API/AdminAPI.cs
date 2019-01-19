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
        public string UserId;
        public string Password;
    }

    class UserListRequest : SessionRequest
    {
    }

    class UserListResponse : SessionResponse
    {
        public MongoDb.User[] Users;
    }
    
    class UserResetPasswordResponse : SessionResponse
    {
        public string NewPassword = string.Empty;        
    }

    class SessionListRequest : SessionRequest { }
    class SessionListResponse : SessionResponse
    {
        public MongoDb.UserSession[] Sessions;
    }    
}
