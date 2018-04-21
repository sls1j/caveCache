using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.API
{
    class UserGetInfoRequest : SessionRequest
    {
    }
  

    class UserGetInfoResponse : SessionResponse
    {
        public string Name;
        public string Profile;
        public string Permissions;
        public Database.Data[] Data;
        public Database.Media[] Media;
        public SurveyInfo[] Surveys;
        public CaveInfo[] Caves;
    }
}
