function HomeViewModel(nav, agent) {
    ViewModel(this, "home", nav, agent);

    var public = this;
    var private = public.private;
    var protected = public.protected;

    protected.navigatedTo = function(data) {
        private.doLoadUserdata();
    }

    private.doLoadUserdata = function() {
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
                private.GetMap();
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

        agent.caveAdd().then(response => {
            var cave = response.Cave;
            Cave(cave);
            public.Caves().push(cave);
            private.nav.navigateTo("cave-edit", {method: "edit", cave: cave, userInfo: private.userInfo});
        });
    }

    public.showCave = function() {
        private.nav.navigateTo("cave-show", this);
    }

    public.importCaves = function() {
        private.nav.navigateTo("caves-import", null);
    }

    private.GetMap = function() {
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

        let centerX = 0;
        let centerY = 0;
        // find center of map
        if (private.allCaves.length > 0) {
            let lat = private.allCaves[0].Latitude;
            let lon = private.allCaves[0].Longitude;
            for (let i = 1; i < private.allCaves.length; i++) {
                let c = private.allCaves[i];
                lat += c.Latitude;
                lon += c.Longitude;
            }
            centerX = lon / private.allCaves.length;
            centerY = lat / private.allCaves.length;
        }
        else {
            centerX = 41;
            centerY = -111;
        }

        // make map

        let map = new Microsoft.Maps.Map('#map', {
            credentials: 'AvqRAHT_GY-E5tkeeYC8qFIfEZC_9UGC9SnXhS9Z94KsZhwoV-g-4lmcTFenisSn',
            center: new Microsoft.Maps.Location(centerY, centerX),
            mapTypeId: Microsoft.Maps.MapTypeId.aerial,
            zoom: 10
        });

        // add caves
        for (let i = 0; i < private.allCaves.length; i++) {
            let c = private.allCaves[i];
            let pin = makePin(c.Latitude, c.Longitude, c.Name, () => {
                private.nav.navigateTo("cave-show", c);
            });
            map.entities.push(pin);
        }
    }

    public.search = ko.observable("");
    public.search.subscribe(private.searchCaves);
}