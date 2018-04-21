function HomeViewModel(nav, agent) {
    ViewModel(this, "home", nav, agent);

    var public = this;
    var private = public.private;
    var protected = public.protected;

    protected.navigatedTo = function(data) {
        private.doLoadUserdata();
    }

    private.doLoadUserdata = function() {
        let dp = function(obj, name, getFunc) {
            Object.defineProperty(obj, name, {get: getFunc});
        }

        let mix = function(cave) {
            cave.findLocation = function(id) {
                for (let i = 0; i < cave.Locations.length; i++) {
                    if (cave.Locations[i].LocationId === id)
                        return cave.Locations[i];
                }

                return null;
            }

            dp(cave, "Latitude", () => cave.findLocation(cave.LocationId).Latitude);
            dp(cave, "Longitude", () => cave.findLocation(cave.LocationId).Longitude);
            dp(cave, "Accuracy", () => cave.findLocation(cave.LocationId).Accuracy);
            dp(cave, "Altitude", () => cave.findLocation(cave.LocationId).Altitude);
        }

        private.agent.userGetInfo()
            .then(userInfo => {
                public.Caves.removeAll();
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
        private.agent.removeCave(this.CaveId)
            .then(() => {private.doLoadUserdata()});
    }

    public.editCave = function() {
        private.nav.navigateTo("cave-edit", {method: "edit", cave: this});
    }

    public.editCaveMedia = function() {
        private.nav.navigateTo("cave-media", {method: "normal", cave: this});
    }

    public.addCave = function() {
        private.nav.navigateTo("cave-edit", {method: "new", cave: null});
    }

    public.showCave = function() {
        private.nav.navigateTo("cave-show", this);
    }

    public.importCaves = function() {
        private.nav.navigateTo("caves-import", null);
    }

    public.search = ko.observable("");
    public.search.subscribe(private.searchCaves);
}