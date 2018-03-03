function EditLocationViewModel(nav, agent) {
    ViewModel(this, "location-edit", nav, agent);

    var public = this;
    var private = public.private;
    var protected = public.protected;

    Object.defineProperty(public, "Method", { get: () => private.method });
    public.CaptureDate = ko.observable();
    public.Latitude = ko.observable();
    public.Longitude = ko.observable();
    public.Altitude = ko.observable();
    public.Accuracy = ko.observable();
    public.AltitudeAccuracy = ko.observable();
    public.Unit = ko.observable();
    public.Source = ko.observable();
    public.Notes = ko.observable();
    public.LocationId = 0;

    protected.navigatedTo = function (data) {
        let l = data.data.location;
        public.LocationId = l.LocationId;
        public.CaptureDate(l.CaptureDate);
        public.Latitude(l.Latitude);
        public.Longitude(l.Longitude);
        public.Altitude(l.Altitude);
        public.Accuracy(l.Accuracy);
        public.AltitudeAccuracy(l.AltitudeAccuracy);
        public.Unit(l.Unit);
        public.Source(l.Source);
        public.Notes(l.Notes);
    }

    public.save = function () {
        let l = {};
        l.LocationId = public.LocationId;
        l.CaptureDate = public.CaptureDate();
        l.Latitude = public.Latitude();
        l.Longitude = public.Longitude();
        l.Altitude = public.Altitude();
        l.Accuracy = public.Accuracy();
        l.AltitudeAccuracy = public.AltitudeAccuracy();
        l.Unit = public.Unit();
        l.Source = public.Source();
        l.Notes = public.Notes();

        // navigate back to cave edit page
        private.nav.navigateTo("cave-edit", { method: "add-location", location: l });
    }

    public.cancel = function () {
        // navigate back to cave edit page
        private.nav.navigateTo("cave-edit", { method: "cancel-add-location" });
    }
}