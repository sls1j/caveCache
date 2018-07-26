function ShowCaveViewModel(nav, agent) {
    ViewModel(this, "cave-show", nav, agent)
    var public = this;
    var private = public.private;
    var protected = public.protected;

    public.Cave = null;
    public.DeprivedUsers = ko.observableArray();
    public.SelectedUser = ko.observable();

    protected.navigatedTo = function (data) {
            public.Cave = data.data;

            agent.userGetShareList(public.Cave.CaveId)
                .then( (response)=>{
                    for(let i=0; i < response.Users.length; i++ )
                    {
                        let u = response.Users[i];
                        public.DeprivedUsers.push(u);
                    }
                });

            let grid = document.getElementById("cave-data-grid");
            DynoGrid(grid, false);
            grid.addRows(public.Cave.CaveData);

            private.GetMap();
    }

    protected.navigatingFrom = function (data) {
    }

    public.returnToHome = function(){
        private.nav.navigateTo("home");
    }

    public.getData = function(key){
        if ( public.Cave ){
            let cave = public.Cave.CaveData.find( cd => cd.Key === key);
            if ( cave )
                return cave.Value;
        }

        return "<no value>";
    }

    public.isSelected = function(location){
        return location.LocationId === public.Cave.LocationId;
    }

    public.shareCave = function()
    {
        if ( public.SelectedUser() )
        {
            let u = public.SelectedUser();
            agent.shareCave(public.Cave.CaveId, u.UserId)
                .then( result => alert("Cave shared!"));
        }
    }

    private.GetMap = function()
    {   
        function makeLoc(lat,long)     
        {
            return new Microsoft.Maps.Location(lat,long);
        }

        function makePin(lat,long)
        {
            let loc = makeLoc(lat,long);
            return new Microsoft.Maps.Pushpin(loc,{color: 'green'});
        }

        let map = new Microsoft.Maps.Map('#map', {
            credentials: 'AvqRAHT_GY-E5tkeeYC8qFIfEZC_9UGC9SnXhS9Z94KsZhwoV-g-4lmcTFenisSn',
            center: new Microsoft.Maps.Location(public.Cave.Latitude, public.Cave.Longitude),
            mapTypeId: Microsoft.Maps.MapTypeId.aerial,
            zoom: 14
        });

        let pin = makePin(public.Cave.Latitude, public.Cave.Longitude);
        map.entities.push(pin);
    }
}
