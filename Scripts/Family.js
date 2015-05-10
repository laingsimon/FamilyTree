$(document).ready(function () {
	$(".person .details").click(function () {
		$(".details-popup.visible").removeClass("visible");
		$(".marriage-popup.visible").removeClass("visible");

		$(this).prev(".details-popup").addClass("visible");
		$(document.body).addClass("popup-shown");
	});

	$(".person .details-popup").click(function () {
		$(this).removeClass("visible");
		$(document.body).removeClass("popup-shown");
	});

	$(".marriage-marker").click(function () {
		$(".details-popup.visible").removeClass("visible");
		$(".marriage-popup.visible").removeClass("visible");

		$(this).prev(".marriage-popup").addClass("visible");
		$(document.body).addClass("popup-shown");
	});

	$(".marriage-popup").click(function () {
		$(this).removeClass("visible");
		$(document.body).removeClass("popup-shown");
	});

	var highlight = window.location.hash;
	if (highlight) {
		var idOfElement = highlight.replace("#", "");

		var highlightElement = $("#" + idOfElement + " > .details:not(.details-popup)");

		if (highlightElement.length > 0) {
			highlightElement.trigger("click");
			highlightElement[0].scrollIntoView();
		}
		else {
			alert("Cannot find person to highlight " + highlight);
		}
	}
});