function DynoGrid(element, canEdit) {
    var public = element;
    var private = public.private = {};

    private.canEdit = canEdit;
    private.disabled = !canEdit;

    private.init = function () {
        public.innerHTML = "";

        private.table = document.createElement("table");
        private.table.appendChild(private.createHeaderRow(["Name", "Value", ""]));
        public.appendChild(private.table);

        private.valueIndex = {};
    }

    private.createHeaderRow = function (columns) {
        let row = document.createElement("tr");
        for (let i = 0; i < columns.length; i++) {
            let column = document.createElement("th");
            let textNode = document.createTextNode(columns[i]);
            column.appendChild(textNode);
            row.appendChild(column);
        }

        return row;
    }

    public.addRow = function (name, type, metadata, value) {
        let row = document.createElement("tr");
        let nameColumn = document.createElement("td");
        nameColumn.appendChild(document.createTextNode(name));

        // values
        let valueColumn = document.createElement("td");
        let valueControl = null;
        let valueGetter = null;

        switch (type) {
            case 'number':
            case 'date':
            case 'text':
            default:
                valueControl = document.createElement("input");
                valueControl.type = type;
                valueControl.value = value;
                valueGetter = function(){ return valueControl.value; }
                break
            case 'textarea':
                valueControl = document.createElement("textarea");
                valueControl.value = value;
                valueGetter = function(){ return valueControl.value; }
                break;
            case 'checkbox':
                valueControl = document.createElement("input");
                valueControl.type = type;
                valueControl.checked = value === "true";
                valueGetter = function(){ return valueControl.checked.toString(); }
                if ( private.readOnly )
                    valueControl.disabled = true;
                break;
        }

        if (private.disabled)
            valueControl.readOnly = true;

        valueColumn.appendChild(valueControl);

        // action buttons
        let buttonColumn = null;
        if (private.canEdit) {
            buttonColumn = document.createElement("td");
            let removeButton = document.createElement("button");
            removeButton.innerHTML = "X";
            removeButton.onclick = function (evt) {
                executeMessageBox("Are you sure?", function (result) {
                    if (result)
                        private.removeRow(name);
                });
            };
            buttonColumn.appendChild(removeButton);
        }


        // build row
        row.appendChild(nameColumn);
        row.appendChild(valueColumn);

        if (buttonColumn)
            row.appendChild(buttonColumn);

        private.table.appendChild(row);

        // for building the data coming out
        private.valueIndex[name] = { name: name, type: type, metadata: metadata, row: row, control: valueControl, valueGetter: valueGetter };
    }

    public.addRows = function (data) {
        for (let i = 0; i < data.length; i++) {
            let datum = data[i];
            public.addRow(datum.Name, datum.Type, datum.MetaData, datum.Value);
        }
    }

    public.clearRows = function () {
        privat.table.innerHTML = "";
    }

    private.removeRow = function (name) {
        let field = private.valueIndex[name];
        private.table.removeChild(field.row);
        delete private.valueIndex[name];
    }

    public.valuesAsObject = function () {
        let output = [];
        // extract all the data
        for (key in private.valueIndex) {
            let field = private.valueIndex[key];
            let data = {
                Name: field.name,
                Type: field.type,
                MetaData: field.metadata,
                Value: field.valueGetter()
            };
            output.push(data);
        }

        return output;
    }



    private.init();
}