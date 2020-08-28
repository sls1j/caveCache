using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaveCache.Nouns.Database
{
  public class Media
  {
    public string Id { get; set; }
    public string FileName { get; set; }
    public string Description { get; set; }
    public string MimeType { get; set; }
    public int FileSize { get; set; }
    public string AttachId { get; set; }
    public string AttachType { get; set; }
    public int? OldId { get; set; }

    public Media Clone()
    {
      return MemberwiseClone() as Media;
    }
  }
}
