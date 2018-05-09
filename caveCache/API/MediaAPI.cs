using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace caveCache.API
{
    class CreateMediaRecordRequest : SessionRequest
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

    class SetMediaStreamRequest : SessionRequest
    {
        public int MediaId;
        public Stream InputStream;
    }

    class SetMediaStreamResponse : SessionResponse
    {
        public int MediaId;
    }

    class GetMediaStreamRequest : SessionRequest
    {
        public int MediaId;
    }

    class GetMediaStreamResponse : SessionResponse
    {
        public int MediaId;
        public Stream Stream;
    }
}
