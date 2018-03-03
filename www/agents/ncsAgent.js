function NewCaveSurveyAgent(url) {
    var public = this;
    var private = public.private = {};
    private.url = url;

    private.sessionId = "";
    private.nextRequestId = 1;

    public.login = function (email, password) {
        return new Promise((resolve, reject) => {
            let login = {
                ResquestType: "LoginRequest",
                RequestId: private.nextRequestId++,
                Email: email,
                Password: password
            };
            private.sendCommand(login)
                .then(loginResponse => {
                    if (loginResponse.Status === 200) {
                        private.sessionId = loginResponse.SessionId;
                        resolve(private.sessionId);
                    }
                    else
                        reject("Failed to login");
                },
                (err) => reject("API command failed: " + err.status + "," + err.Error));
        });
    }

    public.userGetInfo = function () {
        return new Promise((resolve, reject) => {
            let userGetInfo = {
                ResquestType: "UserGetInfoRequest",
                SessionId: private.sessionId,
                RequestId: private.nextRequestId++
            };

            private.sendCommand(userGetInfo).then(
                response => {
                    if (response.Status == 200)
                        resolve(response);
                    else
                        reject("UserGetInfoRequest failed: " + response.Status, + " " + response.Error);
                },
                err => reject("API command failed: " + err.status + "," + err, statusText)
            );
        });
    }

    public.caveSave = function (cave, isNew) {
        return new Promise((resolve, reject) => {
            let caveSave = {
                ResquestType: "CaveAddUpdateRequest",
                SessionId: private.sessionId,
                RequestId: private.nextRequestId++,
                CaveId: cave.CaveId,
                Name: cave.Name,
                Description: cave.Description,
                LocationId: cave.LocationId,
                Locations: cave.Locations,
                Data: cave.CaveData
            };

            private.sendCommand(caveSave).then(
                response => {
                    if (response.Status == 200)
                        resolve(response);
                    else
                        reject("CaveAddUpdateRequest failed: " + response.Status, + " " + response.Error);
                },
                err => reject("API command failed: " + err.status + "," + err, statusText)
            );
        });
    }

    private.sendCommand = function (command) {
        return new Promise(function (resolve, reject) {
            var xhr = new XMLHttpRequest();
            let dest = private.url + "/API/" + command["ResquestType"];
            xhr.open("POST", dest);
            xhr.onload = function () {
                console.log("received response");
                if (this.status >= 200 && this.status < 300) {
                    let userInfo = JSON.parse(xhr.response)
                    resolve(userInfo);
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
            let data = JSON.stringify(command);
            console.log(dest, " ", data);
            xhr.send(data);
        });
    }
}

var $Cave_EmptyLocation = {
    LocationId: -1, CaptureDate: null,
    Latitude: 0, Longitude: 0, Altitude: null,
    Accuracy: null, AltitudeAccuracy: null,
    Unit: 0, Source: "", Notes: ""
}

function Location(loc = null) {
    if (null === loc)
        loc = this;

    return Object.deepExtend($Cave_EmptyLocation, loc);
}

function Cave(cave = null) {

    let dp = function (obj, name, getFunc) {
        Object.defineProperty(obj, name, { get: getFunc });
    }

    if (cave === null) {
        cave = this;
        cave.Name = "";
        cave.Description = "";
        cave.LocationId = 1;
        cave.Locations = [new Location()];
        cave.Locations[0].LocationId = 1;
        cave.CaveData = [];
    }

    // fill in helper methods and properties
    cave.findLocation = function (id) {
        for (let i = 0; i < cave.Locations.length; i++) {
            if (cave.Locations[i].LocationId === id)
                return cave.Locations[i];
        }

        return null;
    }

    dp(cave, "Latitude", () => (cave.findLocation(cave.LocationId) || $Cave_EmptyLocation).Latitude);
    dp(cave, "Longitude", () => (cave.findLocation(cave.LocationId) || $Cave_EmptyLocation).Longitude);
    dp(cave, "Accuracy", () => (cave.findLocation(cave.LocationId) || $Cave_EmptyLocation).Accuracy);
    dp(cave, "Altitude", () => (cave.findLocation(cave.LocationId) || $Cave_EmptyLocation).Altitude);
}