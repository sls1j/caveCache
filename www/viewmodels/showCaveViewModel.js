function ShowCaveViewModel(nav, agent) {
    ViewModel(this, "cave-show", nav, agent)
    var public = this;
    var private = public.private;
    var protected = public.protected;

    public.Cave = null;
    public.DeprivedUsers = ko.observableArray();
    public.SelectedUser = ko.observable();

    protected.navigatedTo = function (evt) {
        public.Cave = evt.data.cave;
        private.mapData = evt.data.mapData;
        private.returnTo = evt.from;

        agent.userGetShareList(public.Cave.CaveId)
            .then((response) => {
                public.DeprivedUsers = [];
                for (let i = 0; i < response.Users.length; i++) {
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

    public.returnToHome = function () {
        private.nav.navigateTo(private.returnTo, { mapData: private.mapData });
    }

    public.getData = function (key) {
        if (public.Cave) {
            let cave = public.Cave.CaveData.find(cd => cd.Key === key);
            if (cave)
                return cave.Value;
        }

        return "<no value>";
    }

    public.isSelected = function (location) {
        return location.LocationId === public.Cave.LocationId;
    }

    public.shareCave = function () {
        if (public.SelectedUser()) {
            let u = public.SelectedUser();
            agent.shareCave(public.Cave.CaveId, u.UserId)
                .then(result => alert("Cave shared!"));
        }
    }

    private.GetMap = function () {
        let map = L.map('map').setView([public.Cave.Latitude, public.Cave.Longitude], 13);
        setupMapLayers(map);
        /*
        let geo = L.tileLayer('https://webmaps.geology.utah.gov/arcgis/rest/services/GeolMap/30x60_Quads/MapServer/tile/{z}/{x}/{y}?blankTile=false');
        let geo_topo = L.tileLayer(
            'https://api.mapbox.com/v4/mapbox.outdoors/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoiYnJpYW4tZGlja2V5IiwiYSI6ImNqcjZ6YWhoZzAxank0YXMzNm44YmJsYzUifQ.jBgCFt06nwAWC6jFmDrqVg',
             {"opacity" : 0.8}).addTo(geo);
        
        let geo_group = L.layerGroup([geo,geo_topo]);
        let baseMaps = {
            "Topo" : L.tileLayer('https://api.mapbox.com/v4/mapbox.outdoors/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoiYnJpYW4tZGlja2V5IiwiYSI6ImNqcjZ6YWhoZzAxank0YXMzNm44YmJsYzUifQ.jBgCFt06nwAWC6jFmDrqVg', {}).addTo(map),
            "Sat" :  L.tileLayer('https://api.mapbox.com/v4/mapbox.satellite/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoiYnJpYW4tZGlja2V5IiwiYSI6ImNqcjZ6YWhoZzAxank0YXMzNm44YmJsYzUifQ.jBgCFt06nwAWC6jFmDrqVg', {}),
            "Geo" : geo_topo
        };

        L.control.layers(baseMaps, {}).addTo(map);*/

        private.map = map;

        let locs = [];
        L.marker([public.Cave.Latitude, public.Cave.Longitude]).addTo(map);
    }

    public.toggleNote = function (note, evt) {
        let button = evt.currentTarget;
        let token = button.children[0];
        let collapsed = button.nextElementSibling;
        if (collapsed.style.display === "none") {
            collapsed.style.display = "block";
            token.className = "fas fa-minus-square";
        }
        else {
            collapsed.style.display = "none";
            token.className = "fas fa-plus-square";
        }
    }
}
