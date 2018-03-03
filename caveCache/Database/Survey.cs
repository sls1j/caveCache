using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.Database
{
    public class Survey
    {
        public int SurveyId;
        public string Name;
        public string Description;
        public List<Data> Data;
        public List<int> Media;
    }

    public class SurveyCave
    {
        public int Id;
        public int SurveyId;
        public int CaveId;
    }

    public class SurveyUser
    {
        public int Id;
        public int SurveyId;
        public int UserId;
        public string Permissions;        
    }

    public class Project
    {
        public int ProjectId;
        public int SurveyId;

        public string Name;
        public string Description;
    }
}
