using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.API
{
    class Request
    {
        public string RequestType => this.GetType().Name;
        public int RequestId;
    }
    class SessionRequest : Request
    {
        public string SessionId;
    }

    public class Response
    {
        public string ResponseType => this.GetType().Name;
        public int RequestId;
        public int Status;
        public string Error;
    }

    class SessionResponse : Response
    {
        public string SessionId;

        public void CopySessionInfo(SessionRequest request)
        {
            this.RequestId = request.RequestId;
            this.SessionId = request.SessionId;
        }
    }


    class LoginRequest : Request
    {
        public string Email;
        public string Password;
    }

    class LoginResponse : SessionResponse
    {
    }

    class MediaMetaData
    {
        public int MediaId;
        public string Name;
        public string Description;
        public string FileName;
        public string MimeType;
        public int FileSize;
        public string Url;
    }

}
