function EditNoteViewModel(nav, agent) {
    ViewModel(this, "note-edit", nav, agent);

    var public = this;
    var private = public.private;
    var protected = public.protected;

    Object.defineProperty(public, "Method", { get: () => private.method });
    public.CreatedTimestamp = ko.observable();
    public.Notes = ko.observable();

    protected.navigatedTo = function (data) {
        let n = data.data.note;
        private.note = n;
        public.CreatedTimestamp(n.CreatedTimestamp);
        public.Notes(n.Notes);        
    }

    public.save = function () {
        let n = {};
        let pn = private.note;
        n.NoteId = private.note.NoteId;
        n.CreatedTimestamp = pn.CreatedTimestamp;
        n.Notes = public.Notes();
        n.UserId = pn.UserId;
        n.AttachId = pn.AttachId;
        n.AttachType = pn.AttachType;

        // navigate back to cave edit page
        private.nav.navigateTo("cave-edit", { method: "edit-note", note: n });
    }

    public.cancel = function () {
        // navigate back to cave edit page
        private.nav.navigateTo("cave-edit", { method: "cancel-edit-note" });
    }
}