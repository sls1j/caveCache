function CavesImportViewModel(nav, agent) {
    ViewModel(this, "caves-import", nav, agent);

    var public = this;
    var private = public.private;
    var protected = public.protected;

    protected.navigatedTo = function (data) {
        public.logs("");
        public.csvData("");
    }

    public.importData = function () {
        // get the data from the model
        let data = private.CSVToArray(public.csvData());

        if (data.length <= 1) {
            private.addLog("Not enough data!")
            return;
        }

        // read the header -- make sure that all the required fields are present
        let header = data[0];
        let headerMap = [];
        let requiredHeaderValues = ["Name", "Latitude", "Longitude"];

        for (let i = 0; i < requiredHeaderValues.length; i++) {
            let rh = requiredHeaderValues[i];
            let found = false;
            for (let j = 0; j < header.length; j++) {
                if (header[j] === rh) {
                    found = true;
                    break;
                }
            }

            if (!found) {
                private.addLog('Required header column "' + rh + '" not found.')

                return;
            }
        }
        private.addLog("Header Checks out.")

        // read the values
        let caves = [];
        for (let i = 1; i < data.length; i++) {
            let row = data[i];
            let cave = { CaveData: [], Locations: [{LocationId: 1}], LocationId: 1 };
            for (let j = 0; j < row.length; j++) {
                let fieldName = header[j];
                switch (fieldName) {
                    case "Name":
                    case "Description":
                        cave[fieldName] = row[j];
                        break;
                    case "Latitude":
                    case "Longitude":
                    case "Altitude":
                    case "Accuracy":
                        cave.Locations[0][fieldName] = row[j];
                        break;
                    default:
                        cave.CaveData.push({
                            Name: fieldName,
                            Type: "text",
                            MetaData: null,
                            Value: row[j]
                        });
                        break;
                }
            }
            caves.push(cave);
            console.log(cave);
            private.addLog("Preparing cave: " + cave["Name"]);
        }

        // give a summary of what is being imported and let the user say yes/no        
        executeMessageBox(`Have ${caves.length} caves to add.  Do you want to?`, function (yes) {
            if (yes) {
                // if yes then begin to save the caves on at a time.
                private.addLog("Adding caves");
                private.saveCaves(0, caves);
            }
        });
    }

    private.saveCaves = function (index, caves) {
        if (index >= caves.length) {
            private.nav.navigateTo("home");
            return;
        }

        let cave = caves[index];

        private.agent.caveSave(cave)
            .then((result) => {
                console.log(result);
                private.saveCaves(index + 1, caves);
            },
            () => {

            })
    }

    public.cancelImport = function () {
        private.nav.navigateTo("home");
    }

    public.csvData = ko.observable("");
    public.logs = ko.observable("");

    private.CSVToArray = function (strData, strDelimiter) {
        // Check to see if the delimiter is defined. If not,
        // then default to comma.
        strDelimiter = (strDelimiter || ",");

        // Create a regular expression to parse the CSV values.
        var objPattern = new RegExp(
            (
                // Delimiters.
                "(\\" + strDelimiter + "|\\r?\\n|\\r|^)" +

                // Quoted fields.
                "(?:\"([^\"]*(?:\"\"[^\"]*)*)\"|" +

                // Standard fields.
                "([^\"\\" + strDelimiter + "\\r\\n]*))"
            ),
            "gi"
        );


        // Create an array to hold our data. Give the array
        // a default empty first row.
        var arrData = [[]];

        // Create an array to hold our individual pattern
        // matching groups.
        var arrMatches = null;


        // Keep looping over the regular expression matches
        // until we can no longer find a match.
        while (arrMatches = objPattern.exec(strData)) {

            // Get the delimiter that was found.
            var strMatchedDelimiter = arrMatches[1];

            // Check to see if the given delimiter has a length
            // (is not the start of string) and if it matches
            // field delimiter. If id does not, then we know
            // that this delimiter is a row delimiter.
            if (
                strMatchedDelimiter.length &&
                strMatchedDelimiter !== strDelimiter
            ) {

                // Since we have reached a new row of data,
                // add an empty row to our data array.
                arrData.push([]);

            }

            var strMatchedValue;

            // Now that we have our delimiter out of the way,
            // let's check to see which kind of value we
            // captured (quoted or unquoted).
            if (arrMatches[2]) {

                // We found a quoted value. When we capture
                // this value, unescape any double quotes.
                strMatchedValue = arrMatches[2].replace(
                    new RegExp("\"\"", "g"),
                    "\""
                );

            } else {

                // We found a non-quoted value.
                strMatchedValue = arrMatches[3];

            }


            // Now that we have our value string, let's add
            // it to the data array.
            arrData[arrData.length - 1].push(strMatchedValue);
        }

        // Return the parsed data.
        return (arrData);
    }

    private.caveDataAddRow = function (cave, name, type, meta, value) {
        cave.CaveData.push({
            Name: name,
            Type: type,
            MetaData: meta,
            Value: value
        });
    }

    private.addLog = function (msg) {
        let oldLog = public.logs();
        public.logs(msg + "\r\n" + oldLog);
        console.log(msg);
    }
}