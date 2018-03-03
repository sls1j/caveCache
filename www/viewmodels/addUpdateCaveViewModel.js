function AddUpdateCaveViewModel(nav, ncsAgent) {
    var public = this;
    var private = public.private = {}
    public.pageName = "edit-cave";

    private.nav = nav;
    private.ncsAgent = ncsAgent;

    private.nav.addEventListener("navigated-to", data => private.handleNavigatedTo(data));
    private.nav.addEventListener("navigating-from", data => private.handleNavigatingFrom(data));

    private.handleNavigatedTo = function (data) {
        if (data.to !== public.pageName)
            return;  
        
        let cave = null;
        if (data.data.method === "new"){
            cave = {

            };
        }
        else 
            cave = data.data.cave;

        // set all properties
    }

    private.handleNavigatingFrom = function (data) {
        if (data.from !== public.pageName)
            return;

        // copy all properties to cave object
        
    }

    public.name = ko.observerable("");
    public.latitude = ko.observerable(0);
    public.longitude = ko.observerable(0);
    public.accuracy = ko.observerable(0);
    public.altitude = ko.observerable(0);
    public.description = ko.observerable("");

}