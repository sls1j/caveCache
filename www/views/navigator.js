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
    private.pageStack = [];

    private.fire = function(eventName, data) {
        if (private.eventHandlers.hasOwnProperty(eventName)) {
            var handlers = private.eventHandlers[eventName];
            for (let i = 0; i < handlers.length; i++)
                handlers[i](data);
        }
    }

    public.addEventListener = function(eventName, handler) {
        if (private.eventHandlers.hasOwnProperty(eventName)) {
            private.eventHandlers[eventName].push(handler);
        }
        else
            throw "Event " + eventName + " not supported";
    }

    private.requestPage = function(url, state) {
        return new Promise(function(resolve, reject) {
            var xhr = new XMLHttpRequest();
            xhr.open("GET", url);
            xhr.onload = function() {
                if (this.status >= 200 && this.status < 300) {
                    resolve({html: xhr.response, state: state});
                } else {
                    reject({
                        status: this.status,
                        statusText: xhr.statusText
                    });
                }
            };
            xhr.onerror = function() {
                reject({
                    status: this.status,
                    statusText: xhr.statusText
                });
            };

            xhr.send();
        });
    }

    public.addPage = function(name, url) {
        private.pages.push({name: name, url: url});
    }

    public.navigateTo = function(name, navigateData) {
        // find the page
        let page = private.pages.find(p => p.name === name);
        if (!page)
            throw "Page '" + name + "' not added.";

        // call navigatingFrom
        let lastPage = private.currentPage;
        let data = {from: private.currentPage, to: page.name, cancel: false};
        private.fire("navigating-from", data);

        // check to see if the navigation has been canceled
        if (data.cancel)
            return;

        // load data
        let ctx = {
            page: page,
            lastPage: lastPage,
            navigateData: navigateData,
            mainHtml: null,
            unprocessedTemplates: [],
            outstandingTemplates: 0,
            error: false,
            templates: {},
            promise: null
        };

        ctx.unprocessedTemplates.push(page.url);
        private.loadPage(ctx);
    }

    private.loadPage = function(ctx) {
        if (ctx.unprocessedTemplates.length === 0 && ctx.outstandingTemplates == 0) {
            // finished so do something
            private.pageContainer.innerHTML = private.buildHtml(ctx);
            // call navigated-to
            private.currentPage = ctx.page.name;
            let data2 = {from: ctx.lastPage, to: private.currentPage, data: ctx.navigateData};
            private.fire("navigated-to", data2);
        }
        else {
            while (ctx.unprocessedTemplates.length > 0) {
                var url = ctx.unprocessedTemplates.pop();
                ctx.outstandingTemplates++;
                private.requestPage(url)
                    .then(result => {
                        if (ctx.mainHtml === null)
                            ctx.mainHtml = result.html;
                        else
                            ctx.templates[url] = result.html;
                        private.extractTemplates(result.html, ctx.unprocessedTemplates);
                        ctx.outstandingTemplates--;
                        private.loadPage(ctx);
                    });
            }
        }
    }

    private.extractTemplates = function(html, unprocessedTemplates) {
        var re = /@@(.+?)@@/g; // matches the url enclosed with @@
        while (true) {            
            var result = re.exec(html);
            if (null === result)
                break;
            if (-1 === unprocessedTemplates.indexOf(result[1]))
                unprocessedTemplates.push(result[1]);
        }
    }

    private.buildHtml = function(ctx) {
        var html = ctx.mainHtml;
        var subs = [];
        var re = /@@(.+?)@@/;
        while (true) {
            let s = html.match(re);
            if ( null == s )
                break;
            let key = s[0];
            let url = s[1];
            let replacement = ctx.templates[url];
            html = html.replace(key, replacement);
        }


        return html;
    }
}