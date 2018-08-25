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
        public int UserId;
        public string Name;
        public string Profile;
        public string Permissions;
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

    [Request("Retrieve the profile information of the user.")]
    class UserGetProfileRequest : SessionRequest
    {
    }

    class UserGetProfileResponse : SessionResponse
    {
        public int UserId;
        public string Email;
        public string Name;
        public string Profile;
        public string Permissions;
    }

    [Request("Set the profile information of the user.")]
    class UserSetProfileRequest : SessionRequest
    {
        [Parameter("The login.")]
        public string Email;
        [Parameter("The name of the person")]
        public string Name;
        [Parameter("Information about the person.")]
        public string Profile;
    }

    class UserSetProfileResponse : SessionResponse
    {
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

    [Request("Sets the current user's password.")]
    class UserSetPasswordRequest : SessionRequest
    {
        [Parameter("The old password")]
        public string OldPassword;
        [Parameter("The new password")]
        public string NewPassword;
    }

    class UserSetPasswordResponse : SessionResponse
    {
    }


    [Request("Get a list of users that you can share a cave with.  Excludes individuals that already have access to the cave.")]
    class UserGetShareListRequest : SessionRequest
    {
        [Parameter("The cave that you are interested in sharing.")]
        public int CaveId;
    }

    class UserGetShareListResponse : SessionResponse
    {
        [Parameter("This list of users that you can share the cave with.")]
        public UserShort[] Users;
    }

    class UserShort
    {
        public int UserId;
        public string Name;
        public string Email;

        public UserShort()
        {

        }

        public UserShort(int userId, string name, string email)
        {
            this.UserId = userId;
            this.Name = name;
            this.Email = email;
        }
    }    
}
