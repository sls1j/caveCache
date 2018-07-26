﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.API
{
    [Request("Allocates a caveId and associates the cave with the current user and default values.  This must be done before adding details, custom data, locations, or media")]
    class CaveCreateRequest : SessionRequest
    {
    }

    class CaveCreateResponse : SessionResponse
    {
        [Parameter("The unique cave identifier.")]
        public CaveInfo Cave;
    }

    [Request("Update the details, locations, and custom data of the specified cave.")]
    class CaveUpdateRequest : SessionRequest
    {
        [Parameter("The cave identifier")]
        public int CaveId;
        [Parameter("The name of the cave")]
        public string Name;
        [Parameter("A description of the cave")]
        public string Description;
        [Parameter("The identifier of the location that will be displayed.  A cave may have more than one location depending on collection methods.  Phone, GPS, from Map etc.")]
        public int? LocationId;

        [Parameter("A list of locations.")]
        public Database.CaveLocation[] Locations;
        [Parameter("A list of custom data items.")]
        public Database.Data[] Data;
        [Parameter("Notes pertaining to the cave.")]
        public Database.CaveNote[] Notes;

        public CaveUpdateRequest()
        {
            CaveId = 0;
            Locations = new Database.CaveLocation[0];
            Data = new Database.Data[0];
            Notes = new Database.CaveNote[0];
        }
    }
    
    class CaveUpdateResponse : SessionResponse
    {
        [Parameter("The cave identifier of the cave that was updated.")]
        public int CaveId;
    }

    [Request("Lists all of the visible caves.")]
    class CaveListRequest : SessionRequest
    {
        public bool allCaves; // is ignored if not an admin account        
    }


    class CaveListResponse : SessionResponse
    {
        public CaveInfo[] Caves;
    }

    [Request("Lists all of the visible caves within the given distance.")]
    class CaveListByLocationRequestion : SessionRequest
    {
        [Parameter("The Latitude of the search location")]
        public decimal Latitude;
        [Parameter("The Longigute of the search location")]
        public decimal Longitude;
        [Parameter("The radius of the search distance")]
        public double Distance;
        internal string Unit;
    }

    class CaveListByLocationResponse : SessionResponse
    {
        public CaveInfo[] Caves;
    }

    class CaveInfo
    {
        public int CaveId;
        public string Name;
        public string Description;
        public int? LocationId;

        public Database.Data[] CaveData;
        public Database.CaveLocation[] Locations;
        public Database.CaveNote[] Notes;
    }

    [Request("Removes the cave include *all* of it's data.")]
    class CaveRemoveRequest : SessionRequest
    {
        public CaveRemoveRequest()
        {
        }

        [Parameter("The cave to be removed.")]
        public int CaveId { get; set; }
    }

    class CaveRemoveResponse : SessionResponse
    {
    }

    [Request("Share a cave record with another user.")]
    class CaveShareRequest : SessionRequest
    {
        [Parameter("The identifier of the cave to share.")]
        public int CaveId;
        [Parameter("The identifier of the user to share the cave with.")]
        public int UserId;
        [Parameter("A note for who and why it's being shared.")]
        public string Note;
    }

    class CaveShareResponse : SessionResponse
    {
    }
}
