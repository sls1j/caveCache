function ViewModel(public, name, nav, agent) {
    var private = public.private = {};
    var protected = public.protected = {};

    private.name = name;
    private.nav = nav;
    private.agent = agent;
    private.inner_pageElement = "page-container";

    protected.navigatedTo = null;
    protected.navigatingFrom = null;

    private.inner_navigatedTo = function (data) {
        if (data.to === private.name) {
            let container = document.getElementById(private.inner_pageElement);

            if (protected.navigatedTo)
                protected.navigatedTo(data);

            ko.applyBindings(public, container);
        }
    }

    private.inner_navigatingFrom = function (data) {
        if (data.from === private.name) {

            ko.cleanNode(document.getElementById(private.inner_pageElement));

            if (protected.navigatedFrom) {
                protected.navigatedFrom(data);
            }
        }
    }   

    private.nav.addEventListener("navigated-to", private.inner_navigatedTo);
    private.nav.addEventListener("navigating-from", private.inner_navigatingFrom);

    protected.rebind = function(){
        let container = document.getElementById(private.inner_pageElement);
        ko.cleanNode(container);
        ko.applyBindings(container);
    }
}