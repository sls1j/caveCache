function ProfileViewModel(nav, agent) {
    ViewModel(this, "profile", nav, agent);

    var public = this;
    var private = public.private;
    var protected = public.protected;

    public.Name = ko.observable();
    public.Email = ko.observable();
    public.Profile = ko.observable();
    public.OldPassword = ko.observable();
    public.NewPassword_1 = ko.observable();
    public.NewPassword_2 = ko.observable();


    protected.navigatedTo = function(evt) {
        connect_menu(evt.to, nav);

        agent.userGetProfile()
            .then((response) => {
                public.Email(response.Email);
                public.Name(response.Name);
                public.Profile(response.Profile);
            });
    }

    public.saveProfile = function() {
        console.info("saving profile");
        console.info("Email: " + public.Email());
        console.info("Name: " + public.Name());
        console.info("Profile: " + public.Profile());

        private.agent.userSetProfile(public.Email(), public.Name(), public.Profile());
    }

    public.savePassword = function() {
        console.info("saving password");
        console.info("OldPassword: " + public.OldPassword());
        console.info("NewPassword_1: " + public.NewPassword_1());
        console.info("NewPassword_2: " + public.NewPassword_2());

        let op = public.OldPassword();
        let p1 = public.NewPassword_1();
        let p2 = public.NewPassword_2();

        if (private.invalid(op))
            executeMessageBox_ok("Must enter the old password.", null);
        else if (p1 !== p2)
            executeMessageBox_ok("New passwords not the same.", null);
        else {
            private.agent.setPassword(op, p1)
                .then(result => {
                    public.OldPassword("");
                    public.NewPassword_1("");
                    public.NewPassword_2("");
                    executeMessageBox_ok("New password set.");
                }, error => executeMessageBox_ok("Unable to set the password."));
        }
    }

    private.invalid = function(password) {
        if (password === undefined || password === null || password === "")
            return true;

        return !(/\S/.test(password)); // true if it contains something other than whitespace
    }
}