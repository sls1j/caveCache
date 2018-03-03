function executeMessageBox(message, callback) {
    let messageBox = document.getElementById("messagebox");
    let messageContainer = document.getElementById("messagebox-message");
    let yesButton = document.getElementById("messagebox-yes");
    let noButton = document.getElementById("messagebox-no");

    yesButton.onclick = function (evt) {
        callback(true);
        messageBox.style.display = "none";
    };

    noButton.onclick = function (evt) {
        callback(false);
        messageBox.style.display = "none";
    };

    messageContainer.innerHTML = "";
    messageContainer.appendChild(document.createTextNode(message));
    messageBox.style.display = "block";

}