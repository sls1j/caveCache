using MongoDB.Bson;
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
    public ObjectId AttachId;
    public string FileName;
    public string MimeType;
    public int FileSize;
    public Stream Media;
  }

  class SetMediaResponse : SessionResponse
  {
    public ObjectId? MediaId;
  }

  [HelpIgnore]
  class GetMediaRequest : SessionRequest
  {
    public ObjectId? MediaId;
    public int? OldMediaId;
  }

  class GetMediaResponse : SessionResponse
  {
    public ObjectId? MediaId;
    public int? OldMediaId;
    public Stream Stream;
  }

  class CleanMediaRequest : SessionRequest
  {
  }

  class CleanMediaResponse : SessionResponse
  {
  }
}
