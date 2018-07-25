using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.API
{
    [Request("Starts a user session.")]
    class LoginRequest : Request
    {
        [Parameter("The email of the user.  Email address is used as the user identifier.")]
        public string Email;
        [Parameter("The password of the user.  https is used which is why this is not encrypted or hashed in the message.")]
        public string Password;
    }

    class LoginResponse : SessionResponse
    {
    }

    [Request("Retrieve all the user, survey, media, and cave data associated with the current session")]
    class UserGetInfoRequest : SessionRequest
    {
    }
  

    class UserGetInfoResponse : SessionResponse
    {
        public int UserId;
        public string Name;
        public string Profile;
        public string Permissions;
        public Database.Data[] Data;
        public CaveInfo[] Caves;
    }

    [Request("Begins the process to reset the user's password.  If successful a verification link is sent via email.")]
    class UserResetPasswordRequest : Request
    {
        [Parameter("The email of the user account to reset the password for.")]
        public string Email;
    }

    class UserResetPasswordRespone : Response
    {        
    }

    [Request("Finishes the process to reset a password.  The verification token must come from the link that is emailed by the UserResetPasswordRequest.  It will generate a new password and email it to the user.")]
    class UserVerifyPasswordResetRequest : Request
    {
        public string VerifyToken = string.Empty;
    }
    
    class UserVerifyPasswordRestResponse : Response
    {        
    }
}
