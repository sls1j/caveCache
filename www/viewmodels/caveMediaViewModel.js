function CaveMediaViewModel(nav, agent) {
    ViewModel(this, "cave-edit-media", nav, agent);

    var public = this;
    var private = public.private;
    var protected = public.protected;

    protected.navigatedTo = function(data) {
        private.method = data.data.method;

        if (private.method === "add") {
                public.Cave = Object.deepClone(data.data.cave);                
                public.Media = {
                    Name: "",
                    Description:"",
                    Source:""
                };
                console.info(public.Cave);
        }
        else {
            console.error("Unknown method '"+data.method + "' don't know how to handle.");
        }
    }    

    public.uploadNewMedia = function()
    {
        console.log("upload")
    }

    public.returnToHome = function() {
        private.nav.navigateTo("home");
    }


    public.fileUpload = function(data, event) {
        
        var fin = event.currentTarget.files[0];

        if (null != fin && fin.size < 5*1024*1024) {

        }
    }
}