using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CaveCache.Nouns.Http
{
    public class Response
    {
        public int Status;
        public string StatusDescription;
        public void SetSuccess()
        {
            Status = (int)HttpStatusCode.OK;
            StatusDescription = "OK";
        }

        public void SetFail(string statusDescription)
        {
            Status = (int)HttpStatusCode.BadRequest;
            StatusDescription = statusDescription;
        }

        public void SetFail(HttpStatusCode code, string statusDescription)
        {
            Status = (int)code;
            StatusDescription = statusDescription;
        }
    }
}
