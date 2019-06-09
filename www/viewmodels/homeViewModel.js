function HomeViewModel(nav, agent) {
    ViewModel(this, "home", nav, agent);

    var public = this;
    var private = public.private;
    var protected = public.protected;

    protected.navigatedTo = function(evt) {
        if (evt.data)
            private.doLoadUserdata(evt.data.mapData);
        else
            private.doLoadUserdata();
    }

    private.doLoadUserdata = function(mapData) {
        private.agent.userGetInfo()
            .then(userInfo => {
                public.Caves.removeAll();
                private.userInfo = userInfo;
                userInfo.Caves.forEach(c => {
                    Cave(c);
                    // sometimes the locationId is bad if so set to valid location
                    if (c.findLocation(c.LocationId) == null) {
                        if (c.Locations.length > 0) {
                            c.LocationId = c.Locations[0].LocationId;
                        }
                    }
                    public.Caves.push(c);
                });
                private.allCaves = userInfo.Caves;
                console.log(userInfo);
                private.GetMap(mapData);
            },
                rejectData => {

                });
    }

    private.searchCaves = function() {
        public.Caves.removeAll();

        let search = public.search();
        if (search === "" || search === null)
            private.allCaves.forEach(c => public.Caves.push(c));
        else {
            if (private.allCaves) {
                let ls = search.toLowerCase();
                for (let i = 0; i < private.allCaves.length; i++) {
                    let c = private.allCaves[i];
                    let cn = c.Name;
                    if (cn.toLowerCase().indexOf(ls) >= 0) {
                        public.Caves.push(c);
                    }
                }
            }
        }
    }


    public.loadData = function() {
        private.doLoadUserdata();
    }

    public.logOut = function() {
        private.nav.navigateTo("login");
    }

    public.Caves = ko.observableArray();

    public.removeCave = function() {
        // ask are you sure?
        var caveId = this.CaveId;
        executeMessageBox("Are you sure you want to delete the cave.  It will delete everything.",
            () => {
                private.agent.caveRemove(caveId)
                    .then(() => {private.doLoadUserdata()});
            });
    }

    public.editCave = function() {
        private.nav.navigateTo("cave-edit", {method: "edit", cave: this, userInfo: private.userInfo});
    }

    public.addCave = function() {
        var cave = {
            Name = "New Cave",
            Description = "",
            CreatedDate = new Date(),            
        };
        
        Cave(cave);
        public.Caves().push(cave);
        private.nav.navigateTo("cave-edit", {method: "edit", cave: cave, userInfo: private.userInfo});
    }

    public.showCave = function() {
        private.nav.navigateTo("cave-show", {cave: this, mapData: private.map.getBounds()});
    }

    public.importCaves = function() {
        private.nav.navigateTo("caves-import", null);
    }

    private.GetMap = function(mapData) {
        function makeLoc(lat, long) {
            return new Microsoft.Maps.Location(lat, long);
        }

        function makePin(lat, long, title, callback) {
            let loc = makeLoc(lat, long);
            let pin = new Microsoft.Maps.Pushpin(loc, {
                color: 'green',
                title: title,
                enableHoverStyle: true,
                typeName: 'home-push-pin'
            });

            Microsoft.Maps.Events.addHandler(pin, 'click', callback);

            return pin;
        }

        let map = new Microsoft.Maps.Map('#map', {
            credentials: 'AvqRAHT_GY-E5tkeeYC8qFIfEZC_9UGC9SnXhS9Z94KsZhwoV-g-4lmcTFenisSn',
            mapTypeId: Microsoft.Maps.MapTypeId.aerial,
        });

        private.map = map;

        if (mapData !== undefined) {
            map.setView({bounds: mapData, padding: 0});
        }
        else {
            let locs = [];
            for (let i = 0; i < private.allCaves.length; i++) {
                let c = private.allCaves[i];
                if (c.Latitude != 0 || c.Longitude != 0) {
                    var loc = new Microsoft.Maps.Location(c.Latitude, c.Longitude);
                    locs.push(loc);
                }
            }
            var rect = Microsoft.Maps.LocationRect.fromLocations(locs);
            map.setView({bounds: rect, padding: 80});
        }



        // add caves
        for (let i = 0; i < private.allCaves.length; i++) {
            let c = private.allCaves[i];
            if (c.Latitude != 0 || c.Longitude != 0) {
                let pin = makePin(c.Latitude, c.Longitude, c.Name, () => {
                    private.nav.navigateTo("cave-show", {cave: c, mapData: private.map.getBounds()});
                });
                map.entities.push(pin);
            }
        }
    }

    public.search = ko.observable("");
    public.search.subscribe(private.searchCaves);
}