function ImageLoader() {
	var public = this;
	var private = public.private = {};

	public.constructor = function() {
		let params = new URLSearchParams(window.location.search.substring(1));

		let image_properties = private.image_properties = {};
		image_properties.sessionId = params.get("sessionId");
		image_properties.mediaAttachmentType = params.get("mediaAttachmentType");
		image_properties.mediaAttachmentHandle = params.get("mediaAttachmentHandle");

		private.fileLoader = document.getElementById("fileLoader");
		private.fileLoader.addEventListener('change', public.newFileLoaded, false);

		let url = "";
		if (window.location.hostname === "localhost")
		{
			url = "http://localhost:1222";
			private.isTest = true;
		}
		else
		{
			url = window.location.protocol + "//" + (window.location.host || "localhost");
			private.isTest = false;
		}

		private.url = url;

		console.log(image_properties);

		var n = WYSIWYG_Popup.getParam('wysiwyg');

		// get selection and range
		var sel = WYSIWYG.getSelection(n);
		var range = WYSIWYG.getRange(sel);

		// the current tag of range
		var img = WYSIWYG.findParent("img", range);

		// if no image is defined then return
		if (img == null) return;

		// assign the values to the form elements
		for (var i = 0; i < img.attributes.length; i++) {
			var attr = img.attributes[i].name.toLowerCase();
			var value = img.attributes[i].value;
			alert(attr + " = " + value);
			if (attr && value && value != "null") {
				switch (attr) {
					case "src":
						// strip off urls on IE
						if (WYSIWYG_Core.isMSIE) value = WYSIWYG.stripURLPath(n, value, false);
						document.getElementById('src').value = value;
						break;
					case "alt":
						document.getElementById('alt').value = value;
						break;
					case "align":
						selectItemByValue(document.getElementById('align'), value);
						break;
					case "border":
						document.getElementById('border').value = value;
						break;
					case "hspace":
						document.getElementById('hspace').value = value;
						break;
					case "vspace":
						document.getElementById('vspace').value = value;
						break;
					case "width":
						document.getElementById('width').value = value;
						break;
					case "height":
						document.getElementById('height').value = value;
						break;
					case "session":
						document.getElementById('session').value = value;
						break;
					case "media-attachment-type":
						document.getElementById('media-attachment-type').value = value;
						break;
				}
			}

			// get width and height from style attribute in none IE browsers
			if (!WYSIWYG_Core.isMSIE && document.getElementById('width').value == "" && document.getElementById('width').value == "") {
				document.getElementById('width').value = img.style.width.replace(/px/, "");
				document.getElementById('height').value = img.style.height.replace(/px/, "");
			}
		}
	}

	/* ---------------------------------------------------------------------- *\
		Function    : insertImage()
		Description : Inserts image into the WYSIWYG.
	\* ---------------------------------------------------------------------- */
	public.insertImage = function() {
		var n = WYSIWYG_Popup.getParam('wysiwyg');

		// get values from form fields
		var src = document.getElementById('src').value;
		var alt = document.getElementById('alt').value;
		var width = document.getElementById('width').value
		var height = document.getElementById('height').value
		var border = document.getElementById('border').value
		var align = document.getElementById('align').value
		var vspace = document.getElementById('vspace').value
		var hspace = document.getElementById('hspace').value

		// insert image
		WYSIWYG.insertImage(src, width, height, align, border, alt, hspace, vspace, n);
		window.close();
	}


	/* ---------------------------------------------------------------------- *\
	  Function    : selectItem()
	  Description : Select an item of an select box element by value.
	\* ---------------------------------------------------------------------- */
	public.selectItemByValue = function(element, value) {
		if (element.options.length) {
			for (var i = 0; i < element.options.length; i++) {
				if (element.options[i].value == value) {
					element.options[i].selected = true;
				}
			}
		}
	}

	public.Upload = function() {
		console.log("before click");
		simulatedClick(private.fileLoader);
		console.log("after click");
	}

	public.newFileLoaded = function(evt) {
		console.log("file change");
		// load the file

		let file = evt.target.files[0];
		console.log(file);

		// parse the results to get the new url
		private.readFile(file)
			.then(data => {
				private.sendFile(file.name, file.type, data)
					.then( url => {
						let src = document.getElementById("src");
						src.value = url;
					});
			});
	}

	private.readFile = function(file) {
		return new Promise(function(resolve, reject) {
			var reader = new FileReader();
			reader.onloadend = function() {
				resolve(reader.result);
			}
			reader.onerror = function() {
				reject("failed");
			}

			reader.readAsArrayBuffer(file);
		})
	}

	private.sendFile = function(fileName, type, data) {
		return new Promise(function(resolve, reject) {
			// post the image to the server		
			let xhr = new XMLHttpRequest();
			xhr.open("POST", private.url + "/Media");
			xhr.setRequestHeader("CC-SessionId", private.image_properties.sessionId);
			xhr.setRequestHeader("CC-AttachType", private.image_properties.mediaAttachmentType);
			xhr.setRequestHeader("CC-AttachId", private.image_properties.mediaAttachmentHandle);
			xhr.setRequestHeader("CC-FileName", fileName);
			xhr.setRequestHeader("Content-Type", type);

			xhr.onload = function() {
				console.log("received response");
				if (this.status >= 200 && this.status < 300) {
					let url = "/Media/" + xhr.response;
					resolve(url);
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
			xhr.send(data);
		})
	}


	public.constructor();
}

function simulatedClick(el) {
	var evt;
	if (document.createEvent) {
		evt = document.createEvent("MouseEvents");
		evt.initMouseEvent("click", true, true, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
	}
	(evt) ? el.dispatchEvent(evt) : (el.click && el.click());
}