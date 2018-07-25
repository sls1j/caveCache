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
        
        document.getElementById("ce_notes").value = note.Notes;
        wysiwygSettings.ImagePopupExtraUrlParameters = "sessionId="+encodeURIComponent(agent.sessionId())+"&mediaAttachmentHandle="+note.CaveId + "&mediaAttachmentType=cave";
        WYSIWYG.attachAll(wysiwygSettings);      
    }

    public.save = function () {
        WYSIWYG.updateTextArea("ce_notes");

        let n = new Note();
        let orignalNote = private.note;
        n.NoteId = orignalNote.NoteId;
        n.CreatedDate = orignalNote.CreatedDate;
        n.Notes = document.getElementById("ce_notes").value;
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