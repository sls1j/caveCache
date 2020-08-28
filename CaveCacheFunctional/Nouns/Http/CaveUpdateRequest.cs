using CaveCache.Nouns.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaveCache.Nouns.Http
{
  internal class CaveUpdateRequest
  {
    public string CaveId;
    public string Name;
    public string Description;
    public int? LocationId;
    public CaveSubType SubType;
    public CaveLocation[] Locations;
    public Data[] Data;
    public CaveNote[] Notes;

    public CaveUpdateRequest()
    {
      Locations = new CaveLocation[0];
      Data = new Data[0];
      Notes = new CaveNote[0];
    }
  }
}
