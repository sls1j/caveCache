using caveCache.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.API
{
    class DataTemplateAddUpdateRequest : SessionRequest
    {
        public int? Id;
        public string Name;
        public string Description;
        public DataTemplateItem[] Items;
    }

    class DataTemplateAddUpdateResponse : SessionResponse
    {
        public int Id;
    }

    class DataTemplateGetRequest : SessionRequest
    {
        public int[] TemplateIds;
    }

    class DataTemplateGetResponse : SessionResponse
    {
        public DataTemplate[] Templates;
    }

    class DataTemplateDeleteRequest : SessionRequest
    {
        public int Id;
    }

    class DataTemplateDeleteResponse : SessionResponse
    {
    }

    class AttachTemplateToCaveRequest : SessionRequest
    {
        public int CaveId;
        public int DataTemplateId;
    }

    class AttachTemplateToCaveResponse : SessionRequest
    {
        public CaveInfoFull Cave;
    }
}
