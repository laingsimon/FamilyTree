$(document).ready(function () {
	var highlight = window.location.hash;
	if (highlight) {
		var highlightElement = $("a[name = '" + highlight.replace("#", "") + "']");

		if (highlightElement.length > 0) {
			highlightElement.next()[0].scrollIntoView();
			highlightElement.next().addClass("highlight");
		}
		else {
			alert("Cannot find person to highlight " + highlight);
		}
	}

	var doAlert = function (message, title, callback, image) {
		if (domMessage)
			return domMessage({
				message: message,
				title: title,
				okCallback: callback,
				image: image,
				okText: "Close"
			});

		alert(message);

		if (callback)
			callback();
	};

	var doConfirm = function (message, title, okCallback, cancelCallback, image) {
		if (domMessage)
			return domMessage({
				message: message,
				title: title,
				okCallback: okCallback,
				cancelCallback: cancelCallback,
				image: image,
				okText: "Yes",
				cancelText: "No"
			});

		if (confirm(message)) {
			if (okCallback)
				okCallback();
		}
		else {
			if (cancelCallback)
				cancelCallback();
		}
	};

	$(".particulars").click(function (event) {
		var fullName = $(this).attr("fullName");
		var details = "";
		var thisFamily = $("body").attr("family");
		var lastName = $(this).find("span.last-name").text();
		var dateOfBirth = $(this).find("span.date-of-birth").text();
		var dateOfDeath = $(this).find("span.date-of-death").text();
		var treeAvailable = $(this).attr("tree-available") === "true";
		var photoUri = $($(this)[0]).css("background-image").replace(")", "").replace("url(", "").replace("/h50", "/h200");
		var handle = $(this).prev("a.handle");

		if (dateOfBirth)
			details += "Born: " + dateOfBirth + "\n";

		if (dateOfDeath)
			details += "Died: " + dateOfDeath + "\n";

		event.stopPropagation();

		if ($(event.target).hasClass("hint"))
			return;

		if (!(lastName != thisFamily && lastName != "?" && treeAvailable)) {
			if (event.ctrlKey)
				editMember($(this));
			else
				doAlert(details, fullName, undefined, photoUri);
			return;
		}

		if (event.ctrlKey)
			editMember($(this));
		else
			doConfirm(details + "\n Open the " + lastName + " family tree?", fullName, function () {
			//var dateHandle = dateOfBirth ? "_" + dateOfBirth : "";
			//var handle = fullName.replace(/\ /g, "-") + dateHandle.replace(/\//g, "");

			document.location = lastName + "#" + handle.attr("name");
		}, function () { }, photoUri);
	});

	function editMember(particulars) {
		window.editMember(particulars.prev(".handle"));
	}

	$(".marriage").click(function (event) {
		var date = $(this).attr("date");
		var person = $(this).find(".people .person:first-child div.particulars").attr("fullName");
		var spouse = $(this).find(".people .person:last-child div.particulars").attr("fullName");
		var details = person + " to " + spouse;
		var children = $(this).attr("children");

		if (date)
			details += "\non " + date;

		if (children && children != "0")
			details += "\nhad " + children + " child(ren)";

		event.stopPropagation();

		if ($(event.target).hasClass("hint"))
			return;

		doAlert(details, "Marriage", undefined, null);
	});
});