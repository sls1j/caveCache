function executeMessageBox(message, callback) {
    let messageBox = document.getElementById("messagebox");
    let messageContainer = document.getElementById("messagebox-message");
    let yesButton = document.getElementById("messagebox-yes");
    let noButton = document.getElementById("messagebox-no");

    yesButton.style.display = "block";
    yesButton.innerHTML = "Yes";
    noButton.style.display = "block";

    yesButton.onclick = function(evt) {
        if (callback)
            callback(true);
        messageBox.style.display = "none";
    };

    noButton.onclick = function(evt) {
        callback(false);
        messageBox.style.display = "none";
    };

    messageContainer.innerHTML = "";
    messageContainer.appendChild(document.createTextNode(message));
    messageBox.style.display = "block";

}

function executeMessageBox_ok(message, callback) {
    let messageBox = document.getElementById("messagebox");
    let messageContainer = document.getElementById("messagebox-message");
    let yesButton = document.getElementById("messagebox-yes");
    let noButton = document.getElementById("messagebox-no");

    yesButton.style.display = "block";
    yesButton.innerHTML = "OK";
    noButton.style.display = "none";

    yesButton.onclick = function(evt) {
        if (callback)
            callback(true);
        messageBox.style.display = "none";
    };

    messageContainer.innerHTML = "";
    messageContainer.appendChild(document.createTextNode(message));
    messageBox.style.display = "block";

}