using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.API
{
    class CaveAddUpdateRequest : SessionRequest
    {
        public int? CaveId;
        public string Name;
        public string Description;
        public int LocationId;

        public Database.CaveLocation[] Locations;
        public Database.Data[] Data;

        public CaveAddUpdateRequest()
        {
            Locations = new Database.CaveLocation[0];
            Data = new Database.Data[0];
        }
    }

    class CaveAddUpdateResponse : SessionResponse
    {
        public int CaveId;
    }

    class CaveDataUpdateRequest : SessionRequest
    {
        public int CaveId;
        public Database.Data[] CaveData;
        public bool ReplaceAll;
    }   

    class CaveListRequest : SessionRequest
    {
        public bool allCaves; // is ignored if not an admin account        
    }

    class CaveListResponse : SessionResponse
    {
        public Database.Cave[] Caves;
    }

    class CaveListByLocationRequestion : SessionRequest
    {
        public string Lattitude;
        public string Longitude;
    }

    class CaveListByLocationResponse : CaveListResponse
    {
    }

    class CaveInfoShort
    {
        public int CaveId;
        public string Name;
        public string Description;
        public int? LocationId;

        public Database.Data[] CaveData;
        public Database.CaveLocation[] Locations;

        public CaveInfoShort()
        {
            Locations = new Database.CaveLocation[0];
            CaveData = new Database.Data[0];
        }
    }

    class CaveInfoFull
    {
        public int CaveId;
        public string Name;
        public string Description;
        public string Latitude;
        public string Longitude; // these are string because of how javascript handles numbers as float.  Not enough accuracy.
        public int? Accuracy;
        public int? Altitude;

        public Database.CaveLocation[] Locations;
        public Database.Data[] CaveData;
        public MediaData[] Media;

        public CaveInfoFull()
        {
            Locations = new Database.CaveLocation[0];
            CaveData = new Database.Data[0];
        }
    }    
}
