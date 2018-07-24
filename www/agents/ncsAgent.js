function NewCaveSurveyAgent(url) {
    var public = this;
    var private = public.private = {};
    private.url = url;

    private.sessionId = "";
    private.nextRequestId = 1;

    public.sessionId = () => private.sessionId;

    public.login = function(email, password) {
        return new Promise((resolve, reject) => {
            let login = {
                RequestType: "LoginRequest",
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
                    (err) => reject("API command failed: " + err.status + "," + err.StatusDescription));
        });
    }

    public.userGetInfo = function() {
        return new Promise((resolve, reject) => {
            let userGetInfo = {
                RequestType: "UserGetInfoRequest",
                SessionId: private.sessionId,
                RequestId: private.nextRequestId++
            };

            private.sendCommand(userGetInfo).then(
                response => {
                    if (response.Status == 200)
                        resolve(response);
                    else
                        reject("UserGetInfoRequest failed: " + response.Status, + " " + response.StatusDescription);
                },
                err => reject("API command failed: " + err.status + "," + err, statusText)
            );
        });
    }

    public.caveAdd = function() {
        return new Promise((resolve, reject) => {
            let newCave = {
                RequestType: "CaveCreateRequest",
                SessionId: private.sessionId,
                RequestId: private.nextRequestId
            };

            private.sendCommand(newCave).then(
                response => {
                    if (response.Status == 200)
                        resolve(response);
                    else
                        reject("CaveCreateRequest failed: " + response.Status + " " + response.StatusDescription);
                },
                err => reject("API command failed: " + err.status + " " + err.statusText)
            )
        });
    }

    public.caveUpdate = function(cave) {
        return new Promise((resolve, reject) => {
            let caveUpdate = {
                RequestType: "CaveUpdateRequest",
                SessionId: private.sessionId,
                RequestId: private.nextRequestId++,
                CaveId: cave.CaveId,
                Name: cave.Name,
                Description: cave.Description,
                LocationId: cave.LocationId,
                Locations: cave.Locations,
                Data: cave.CaveData,
                Notes: cave.Notes,
            };

            private.sendCommand(caveUpdate).then(
                response => {
                    if (response.Status == 200)
                        resolve(response);
                    else
                        reject("CaveAddUpdateRequest failed: " + response.Status + " " + response.StatusDescription);
                },
                err => reject("API command failed: " + err.status + "," + err.statusText)
            );
        });
    }

    public.caveRemove = function(caveId) {
        return new Promise((resolve, reject) => {
            let caveRemove = {
                RequestType: "CaveRemoveRequest",
                SessionId: private.sessionId,
                RequestId: private.nextRequestId++,
                CaveId: caveId
            };

            private.sendCommand(caveRemove).then(
                response => {
                    if (response.Status == 200)
                        resolve(response);
                    else
                        reject("CaveRemoveRequest failed:  " + response.Status + " " + response.StatusDescription);
                },
                err => reject("API command failed: " + err.status + "," + err, statusText)

            );
        });
    }

    public.caveGetMedia = function(caveId) {
        return new Promise((resolve, reject) => {
            reject("Not yet implemented");
        });
    }

    public.mediaSave = function(attachType, attachId, name, description, file) {
        return new Promise((resolve, reject) => {
            let mediaSave = {
                RequestType: "CreateMediaRecordRequets",
                SessionId: private.sessionId,
                RequestId: private.nextRequestId++,
                AttachType: attachType,
                AttachToId: attachId,
                Name: name,
                Description: description,
                FileName: file.FileName,
                MimeType: private.GetMimeType(file),
                FileSize: file.Size
            };

            var failed = function(error) {
                reject("API command '" + mediaSave.RequestType + "'failed: " + err.status + "," + err, statusText);
            }

            var streamMedia = function(response) {
                var mediaId = response.MediaId;
                var mime = mediaSave.MimeType;
                var reader = new FileReader();
                reader.onload = function() {
                    private.sendFile(mediaId, mime, reader.result)
                }
                reader.readAsArrayBuffer(event.currentTarget.files[0]);


            }

            var allFinished = function(response) {
                if (response.status == 200)
                    resolve(response);
                else
                    reject("API command '" + mediaSave.RequestType + "'failed: " + response.status + "," + response, statusText);
            }

            private.sendCommand(mediaSave)
                .then(streamMedia, failed)
                .then(allFinished, failed);
        })
    }

    private.GetMimeType = function(file) {
        // get file extension
        // guess via extension
        switch (ext) {
            case "jpg": return "image/jpeg";
            case "bm": case "bmp": return "image/bmp";

        }
        return "applicatoin/octet-stream";
    }

    private.sendCommand = function(command) {
        return new Promise(function(resolve, reject) {
            var xhr = new XMLHttpRequest();
            let dest = private.url + "/API/" + command["RequestType"];
            xhr.open("POST", dest);
            xhr.onload = function() {
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
            xhr.onerror = function() {
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

    private.sendFile = function(mediaId, mime, data) {
        return new Promise(function(resolve, reject) {
            var xhr = new XMLHttpRequest();
            let dest = private.url + "/Media/" + mediaId.toString();
            xhr.open("POST", dest);
            xhr.onload = function() {
                console.log("received response");
                if (this.status >= 200 && this.status < 300) {
                    resolve({
                        status: this.status,
                        statusText: xhr.statusText
                    });
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
            xhr.setRequestHeader("Content-Type", mime);
            xhr.setRequestHeader("X-CaveCache-SessionId", private.sessionId);
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

    let dp = function(obj, name, getFunc) {
        Object.defineProperty(obj, name, {get: getFunc});
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
    cave.findLocation = function(id) {
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