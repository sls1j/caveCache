function LoginViewModel(nav, agent) {
    ViewModel(this, "login", nav, agent);
    var public = this;
    var private = public.private;
    var protected = public.protected;

    protected.navigatedTo = function (data) {
        let button = document.getElementById("login");

        button.addEventListener("click", evt => {
            let email = document.getElementById("email").value;
            let password = document.getElementById("password").value;
            private.agent.login(email, password)
                .then(sessionId => {
                    console.log("logged in with sessionId: ", sessionId);
                    private.nav.navigateTo("home");
                },
                reason => {
                    console.log("failed to login with reason: ", reason);
                });
        });

    }
}