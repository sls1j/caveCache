function MapViewModel(nav, agent) {
    ViewModel(this, "map", nav, agent);

    var public = this;
    var private = public.private;
    var protected = public.protected;

    protected.navigatedTo = function (evt) {
        connect_menu(evt.to, nav);
        if (evt.data)
            private.doLoadUserdata(evt.data.mapData);
        else
            private.doLoadUserdata();
    }

    private.doLoadUserdata = function (mapData) {
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

    public.loadData = function () {
        private.doLoadUserdata();
    }

    public.logOut = function () {
        private.nav.navigateTo("login");
    }

    public.Caves = ko.observableArray();

    public.removeCave = function () {
        // ask are you sure?
        var caveId = this.CaveId;
        executeMessageBox("Are you sure you want to delete the cave.  It will delete everything.",
            () => {
                private.agent.caveRemove(caveId)
                    .then(() => { private.doLoadUserdata() });
            });
    }

    public.editCave = function () {
        private.nav.navigateTo("cave-edit", { method: "edit", cave: this, userInfo: private.userInfo });
    }

    public.addCave = function () {

        agent.caveAdd().then(response => {
            var cave = response.Cave;
            Cave(cave);
            public.Caves().push(cave);
            private.nav.navigateTo("cave-edit", { method: "edit", cave: cave, userInfo: private.userInfo });
        });
    }

    public.showCave = function () {
        private.nav.navigateTo("cave-show", { cave: this, mapData: private.map.getBounds() });
    }

    public.importCaves = function () {
        private.nav.navigateTo("caves-import", null);
    }

    private.GetMap = function (mapData) {
        let map = L.map('map').setView([41.8924361,-111.6350057], 13);
        private.map = map;
        setupMapLayers(map);    

        let locs = [];
        for (let i = 0; i < private.allCaves.length; i++) {
            let c = private.allCaves[i];
            if (c.Latitude != 0 || c.Longitude != 0) {
                let marker = L.marker([c.Latitude, c.Longitude])
                    .addTo(map)
                    .bindPopup(c.Name);

                marker.caveId = c.CaveId;

                marker.on('mouseover', e => e.target.openPopup());
                marker.on('mouseout', e => e.target.closePopup());
                marker.on('click', e => private.nav.navigateTo("cave-show", { cave: c }));
            }
        }
    }

    public.search = ko.observable("");
    public.search.subscribe(private.searchCaves);
}