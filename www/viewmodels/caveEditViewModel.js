function CaveEditViewModel(nav, agent) {
    ViewModel(this, "cave-edit", nav, agent);

    var public = this;
    var private = public.private;
    var protected = public.protected;

    protected.navigatedTo = function(data) {
        private.method = data.data.method;

        if (private.method === "edit" || private.method === "add") {
            if (data.data.method === "edit") {
                public.Cave = Object.deepClone(data.data.cave);

                console.info(public.Cave);
            }            

            if (data.data.location) {
                // had a location to update or add via the locationEditViewModel
                let l = data.data.location;
                let found = false;
                for (let i = 0; i < public.Cave.Locations.length; i++) {
                    var ol = public.Cave.Locations[i];
                    if (public.Cave.LocationId === l.LocationId) {
                        public.Cave.Locations[i] = l;
                        break;
                    }
                }

                if (!found) {
                    // get unique id
                    let maxId = -1;
                    for (let i = 0; i < public.Cave.Locations.length; i++) {
                        var ol = public.Cave.Locations[i];
                        if (maxId < ol.LocationId)
                            maxId = ol.LocationId;
                    }

                    // adding a new location to list
                    l.LocationId = maxId + 1;
                    public.Cave.Locations.push(l);
                }
            }
        }
        else if (private.method === "add-location") {
            // this assumes all the cave data from before the location edit hasn't been cleared out

            // remove old
            public.Cave.Locations.removeAll(l => l.LocationId === data.data.location.LocationId);
            // add new
            let loc = data.data.location;
            if (loc.LocationId === -1) {
                // assign locationId
                let locationNotUnique = true;
                while (locationNotUnique) {
                    locationNotUnique = false;
                    loc.LocationId += 1;
                    for (let i = 0; i < public.Cave.Locations.length; i++) {
                        if (public.Cave.Locations[i].LocationId === loc.LocationId) {
                            locationNotUnique = true;
                            break;
                        }
                    }
                }
            }
            public.Cave.Locations.push(data.data.location);
            // make sure it's sorted
            public.Cave.Locations.sort((a, b) => a.CaptureDate - b.CaptureDate);
        }
        else if (private.method === "cancel-add-location") {
            // this assumes all the cave data from before the location edit hasn't been cleared out
        }
        else if (private.method === "select-active-location") {

        }
        else if (private.method === "add-note")
        {
            // add note via agent
        }

        private.update();
    }

    private.update = function() {        
        // populate the cave data view
        let grid = document.getElementById("cave-data-grid");
        DynoGrid(grid, true);
        grid.addRows(public.Cave.CaveData);   
        
        //document.getElementById("ce_notes").value = public.Cave.Notes;
        //wysiwygSettings.ImagePopupExtraUrlParameters = "sessionId="+encodeURIComponent(agent.sessionId())+"&mediaAttachmentHandle="+public.Cave.CaveId + "&mediaAttachmentType=cave";
        //WYSIWYG.attachAll(wysiwygSettings);      
    }

    public.getData = function(key, defaultValue = "") {
        if (public.Cave) {
            let caveData = public.Cave.CaveData.find(cd => cd.Key === key);
            if (caveData)
                return caveData.Value;
        }

        return defaultValue;

    }

    public.isSelected = function(location) {
        return public.Cave.isSelected(location);
    }

    public.caveDataAddRowType = ko.observable(["number"]);
    public.caveDataAddRowName = ko.observable("");

    public.caveDataAddRow = function() {
        let name = public.caveDataAddRowName();
        let typeSelected = public.caveDataAddRowType();
        let type = "text";
        if (typeSelected.length > 0)
            type = typeSelected[0];
        let grid = document.getElementById("cave-data-grid");

        grid.addRow(name, type, "", null);
    }

    public.returnToHome = function() {
        private.nav.navigateTo("home");
    }

    public.save = function() {      
        WYSIWYG.updateTextArea("ce_notes");        
  
        // disable buttons
        // extract data from the form
        var cave = {
            CaveId: public.Cave.CaveId,
            CaveData: []
        };

        var fields = document.querySelectorAll("[fieldName]");
        for (let i = 0; i < fields.length; i++) {
            let field = fields[i];
            let fieldName = field.getAttribute("fieldName")
            cave[fieldName] = field.value;
        }

        cave.LocationId = public.Cave.LocationId;
        cave.Locations = public.Cave.Locations;

        // get the cave data
        let grid = document.getElementById("cave-data-grid");
        cave.CaveData = grid.valuesAsObject();        

        // send off to the agent
        private.agent.caveUpdate(cave)
            .then(() => {
                private.nav.navigateTo("home");
            },
                () => {});
    }

    public.cancel = function() {
        private.nav.navigateTo("home");
    }

    public.editLocation = function(location) {
        console.info("editLocation: ", location);
        private.nav.navigateTo("location-edit", {location: location});
    }

    public.deleteLocation = function(location) {
        console.info("deleteLocation: ", location);
    }

    public.addLocation = function() {
        private.nav.navigateTo("location-edit", {location: new Location()});
        console.info("addLocation");
    }

    public.selectLocation = function(location) {
        console.info("selectLocation: ", location);
        public.Cave.LocationId = location.LocationId;
        private.nav.navigateTo("cave-edit", {method: "select-active-location"})
    }

    public.addNote = function(){
        let note = new Note();        

        console.info("addNote: ", note)
    }

    public.editNote = function(note){

    }

    public.deleteNote = function(note){
        
    }


    public.isSelected = function(location) {
        var retVal = location.LocationId === public.Cave.LocationId;
        return retVal;
    }

    public.fileUpload = function(data, event) {
        
        var fin = event.currentTarget.files[0];

        if (null != fin && fin.size < 5*1024*1024) {
            var reader = new FileReader();
            reader.onload = function(){
                var preview = document.getElementById("img");
                preview.src = reader.result;
            }
            reader.readAsDataURL(event.currentTarget.files[0]);
        }
    }
}