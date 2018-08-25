function connect_menu(current_location, nav) {
    console.log("connect_menu");
    let menuItems = document.querySelectorAll("[destination]");
    for (let i = 0; i < menuItems.length; i++) {
        let mi = menuItems[i];
        let dest = mi.getAttribute("destination");
        if (dest === current_location) {
            // add code to show that menu item is selected
            mi.className = mi.className + " active";
        }
        else
            mi.addEventListener("click", menu_item_action);
    }

    function menu_item_action(evt) {
        let dest = evt.target.getAttribute("destination");
        nav.navigateTo(dest);
    }
}

