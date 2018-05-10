using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.API
{
    [HelpIgnore]
    class Request
    {        
        [Parameter("The name of the message.")]
        public string RequestType => this.GetType().Name;
        [Parameter("A unique identifier for the request.  This will be repeated int the response allowing the request/response to be matched.")]
        public int RequestId;
    }

    [HelpIgnore]
    class SessionRequest : Request
    {
        [Parameter("A session that identifies the user.  It is returned via the LoginRequest/LoginResponse message")]
        public string SessionId;
    }

    public class Response
    {
        [Parameter("The name of the message")]
        public string ResponseType => this.GetType().Name;
        [Parameter("The unique identifier that was passed in from the request.")]
        public int RequestId;
        [Parameter("The status of the request.  It is based on HTTP status codes. 200 is OK")]
        public int Status;
        [Parameter("If there is an error then this text that describes the error.")]
        public string StatusDescription;
    }

    class SessionResponse : Response
    {
        [Parameter("A session that identifies the user.  It is returned via the LoginRequest/LoginResponse message")]
        public string SessionId;

        public void CopySessionInfo(SessionRequest request)
        {
            this.RequestId = request.RequestId;
            this.SessionId = request.SessionId;
        }
    }
    
    [Request( "Starts a user session.")]
    class LoginRequest : Request
    {
        [Parameter( "The email of the user.  Email address is used as the user identifier.")]
        public string Email;
        [Parameter( "The password of the user.  https is used which is why this is not encrypted or hashed in the message.")]
        public string Password;
    }

    class LoginResponse : SessionResponse
    {
    }

    class RequestAttribute : Attribute
    {
        public string Description { get; set; }
        public Type ResponseType { get; set; }
        public RequestAttribute(string Description)
        {
            this.Description = Description;
        }
    }
    
    class ParameterAttribute : Attribute
    {
        public string Description { get; set; }
        public ParameterAttribute(string Description)
        {
            this.Description = Description;
        }

        public static readonly ParameterAttribute Empty = new ParameterAttribute(string.Empty);

    }

    class HelpIgnoreAttribute : Attribute
    {
    }
}
