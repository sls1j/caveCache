function PageNavigator(pageContainer) {
    var public = this;
    var private = public.private = {};    

    private.eventHandlers = {
        "navigating-from": [],
        "navigated-to": []
    };

    private.pageContainer = pageContainer;
    private.currentPage = null;
    private.pages = [];

    private.fire = function (eventName, data) {
        if (private.eventHandlers.hasOwnProperty(eventName)) {
            var handlers = private.eventHandlers[eventName];
            for (let i = 0; i < handlers.length; i++)
                handlers[i](data);
        }
    }

    public.addEventListener = function (eventName, handler) {
        if (private.eventHandlers.hasOwnProperty(eventName)) {
            private.eventHandlers[eventName].push(handler);            
        }
        else
            throw "Event " + eventName + " not supported";
    }

    private.requestPage = function (url) {
        return new Promise(function (resolve, reject) {
            var xhr = new XMLHttpRequest();
            xhr.open("GET", url);
            xhr.onload = function () {
                if (this.status >= 200 && this.status < 300) {
                    resolve(xhr.response);
                } else {
                    reject({
                        status: this.status,
                        statusText: xhr.statusText
                    });
                }
            };
            xhr.onerror = function () {
                reject({
                    status: this.status,
                    statusText: xhr.statusText
                });
            };

            xhr.send();
        });
    }

    public.addPage = function(name,url){
        private.pages.push({name:name, url:url});
    }

    public.navigateTo = function (name, navigateData) {
        // find the page
        let page =  private.pages.find( p => p.name === name );
        if ( !page )
            throw "Page '" + name + "' not added.";

        // call navigatingFrom
        let lastPage = private.currentPage;
        let data = { from: private.currentPage, to: page.name, cancel: false };
        private.fire("navigating-from", data);

        // check to see if the navigation has been canceled
        if (data.cancel)
            return;

        // load data
        private.requestPage(page.url)
            .then(newPageHtml => {
                // replace old page
                private.pageContainer.innerHTML = newPageHtml;
                // call navigated-to
                private.currentPage = page.name;
                let data2 = { from: lastPage, to: private.currentPage, data: navigateData };
                private.fire("navigated-to", data2);
            });
    }
}