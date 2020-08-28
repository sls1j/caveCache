function EditNoteViewModel(nav, agent) {
    ViewModel(this, "note-edit", nav, agent);

    var public = this;
    var private = public.private;
    var protected = public.protected;

    public.Method = ko.observable();
    public.CreatedDate = ko.observable();
    public.Notes = ko.observable();

    protected.navigatedTo = function (evt) {
        let note = evt.data.note;
        private.note = note;
        public.Method(evt.data.method);
        public.CreatedDate(note.CreatedDate);
        public.Notes(note.Notes);          
        
        var notes = document.getElementById("notes");
        notes.value = note.Notes;
        private.editor = SUNEDITOR.create('notes',{
            // All of the plugins are loaded in the "window.SUNEDITOR" object in dist/suneditor.min.js file
            // Insert options
            // Language global object (default: en)
            //lang: SUNEDITOR_LANG['ko']
        });
    }

    public.save = function () {        
        private.editor.save();
        let notes = document.getElementById("notes");
        console.log(notes.value);
        let n = new Note();
        let orignalNote = private.note;
        n.NoteId = orignalNote.NoteId;
        n.CreatedDate = orignalNote.CreatedDate;
        n.Notes = notes.value;
        n.UserId = orignalNote.UserId;
        n.CaveId = orignalNote.CaveId;

        // navigate back to cave edit page
        private.nav.navigateTo("cave-edit", { method: "edit-note", note: n });
    }

    public.cancel = function () {
        // navigate back to cave edit page
        private.nav.navigateTo("cave-edit", { method: "cancel-edit-note" });
    }
}