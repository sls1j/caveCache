function ShowCaveViewModel(nav, agent) {
    ViewModel(this, "cave-show", nav, agent)
    var public = this;
    var private = public.private;
    var protected = public.protected;

    public.Cave = null;

    protected.navigatedTo = function (data) {
            public.Cave = data.data;

            let grid = document.getElementById("cave-data-grid");
            DynoGrid(grid, false);
            grid.addRows(public.Cave.CaveData);
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

    public.getMapSource = function(){
        return "https://maps.google.com/maps?q="+public.Cave.Latitude+"%20"+public.Cave.Longitude+"&t=k&z=14&ie=UTF8&iwloc=&output=embed";
    }

    public.isSelected = function(location){
        return location.LocationId === public.Cave.LocationId;
    }
}
