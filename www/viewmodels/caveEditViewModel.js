function CaveEditViewModel(nav, agent) {
    ViewModel(this, "cave-edit", nav, agent);

    var public = this;
    var private = public.private;
    var protected = public.protected;

    protected.navigatedTo = function(evt) {
        private.method = evt.data.method;        

        if (private.method === "edit" || private.method === "add") {
            if (evt.data.method === "edit") {
                private.back_to = evt.from;
                public.Cave = Object.deepClone(evt.data.cave);
                private.userInfo = evt.data.userInfo;
                console.info(public.Cave);
            }

            if (evt.data.location) {
                // had a location to update or add via the locationEditViewModel
                let l = evt.data.location;
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
            public.Cave.Locations.removeAll(l => l.LocationId === evt.data.location.LocationId);
            // add new
            let loc = evt.data.location;
            if (loc.LocationId === -1) {
                // assign locationId
                let notUnique = true;
                loc.LocationId = 1;
                while (notUnique) {
                    notUnique = false;
                    for (let i = 0; i < public.Cave.Locations.length; i++) {
                        if (public.Cave.Locations[i].LocationId === loc.LocationId) {
                            notUnique = true;
                            loc.LocationId++;
                            break;
                        }
                    }
                }
            }
            public.Cave.Locations.push(evt.data.location);
            // make sure it's sorted
            public.Cave.Locations.sort((a, b) => a.CaptureDate - b.CaptureDate);
        }
        else if (private.method === "cancel-add-location") {
            // this assumes all the cave data from before the location edit hasn't been cleared out
        }
        else if (private.method === "update") {

        }
        else if (private.method === "edit-note") {
            // remove old
            let note = evt.data.note;
            public.Cave.Notes.removeAll(n => n.NoteId === note.NoteId);
            if (note.NoteId === -1) {
                // assign noteId
                let notUnique = true;
                note.NoteId = 1;
                while (notUnique) {
                    notUnique = false;
                    for (let i = 0; i < public.Cave.Notes.length; i++) {
                        if (note.NoteId === public.Cave.Notes[i].NoteId) {
                            notUnique = true;
                            note.NoteId++; // try the next one
                            break;
                        }
                    }
                }
            }

            //set note summary
            let n = document.createElement("div");
            n.innerHTML = note.Notes;
            let noteText = n.innerText;
            note.Summary = noteText.substring(0, 128);
            if (noteText.length < note.Summary)
                note.Summary = note.Summary + "...";

            public.Cave.Notes.push(note);
            public.Cave.Notes.sort((a, b) => b.CreatedDate - a.CreatedDate);
        }

        private.update();
    }

    private.update = function() {
        // populate the cave data view
        let grid = document.getElementById("cave-data-grid");
        DynoGrid(grid, true);
        grid.addRows(public.Cave.CaveData);
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
        private.nav.navigateTo(private.back_to);
    }

    public.save = function() {
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
        cave.Notes = public.Cave.Notes;

        // get the cave data
        let grid = document.getElementById("cave-data-grid");
        cave.CaveData = grid.valuesAsObject();

        // send off to the agent
        private.agent.caveUpdate(cave)
            .then(() => {
                private.nav.navigateTo(private.back_to);
            },
                () => {});
    }

    public.cancel = function() {
        private.nav.navigateTo(private.back_to);
    }

    public.editLocation = function(location) {
        console.info("editLocation: ", location);
        private.nav.navigateTo("location-edit", {location: location});
    }

    public.deleteLocation = function(location) {
        executeMessageBox("Are you sure you want to delete the location?", (yes) => {
            if (yes) {
                public.Cave.Locations.removeAll(l => l.locationId === location.locationId);
                private.nav.navigateTo("cave-edit", {method: "update"});
                console.info("deleteLocation: ", location);
            }
        });
    }

    public.addLocation = function() {
        private.nav.navigateTo("location-edit", {location: new Location()});
        console.info("addLocation");
    }

    public.selectLocation = function(location) {
        console.info("selectLocation: ", location);
        public.Cave.LocationId = location.LocationId;
        private.nav.navigateTo("cave-edit", {method: "update"});
    }

    public.addNote = function() {
        let note = new Note();
        note.CaveId = public.Cave.CaveId;
        note.UserId = private.userInfo.UserId;
        note.CreatedDate = new Date();
        console.info("addNote: ", note)
        private.nav.navigateTo("note-edit", {note: note, method: "Add"});
    }

    public.editNote = function(note) {
        private.nav.navigateTo("note-edit", {note: note, method: "Edit"});
    }

    public.deleteNote = function(note) {
        executeMessageBox("Are you sure you want to delete the location?", (yes) => {
            if (yes) {
                public.Cave.Notes.removeAll(n => n.NoteId === note.NoteId);
                private.nav.navigateTo("cave-edit", {method: "update"});
            }
        });
    }


    public.isSelected = function(location) {
        var retVal = location.LocationId === public.Cave.LocationId;
        return retVal;
    }

    public.fileUpload = function(data, event) {

        var fin = event.currentTarget.files[0];

        if (null != fin && fin.size < 5 * 1024 * 1024) {
            var reader = new FileReader();
            reader.onload = function() {
                var preview = document.getElementById("img");
                preview.src = reader.result;
            }
            reader.readAsDataURL(event.currentTarget.files[0]);
        }
    }
}