using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace caveCache.API
{
    [HelpIgnore]
    class SetMediaRequest : SessionRequest
    {
        public string AttachType; // user, cave, survey
        public int AttachToId;
        public string Name;
        public string Description;
        public string FileName;
        public string MimeType;
        public int FileSize;
        public Stream Media;
    }

    class SetMediaResponse : SessionResponse
    {
        public int? MediaId;
    }

    [HelpIgnore]
    class GetMediaRequest : SessionRequest
    {
        public int MediaId;
    }

    class GetMediaResponse : SessionResponse
    {
        public int MediaId;
        public Stream Stream;
    }
}
