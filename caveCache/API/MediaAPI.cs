using System;
using System.Collections.Generic;
using System.Text;

namespace caveCache.API
{
    class CreateMediaRecord : SessionRequest
    {
        public string AttachType; // user, cave, survey
        public int AttachToId;
        public string Name;
        public string Description;
        public string FileName;
        public string MimeType;
        public int FileSize;
    }

    class CreateMediaRecordResponse : SessionResponse
    {
        public int? MediaId;
    }
}
