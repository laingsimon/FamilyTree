$(document).ready(function () {
	$(".person .details").click(function (event) {
		var details = $(event.target).closest(".details");
		if (details.hasClass("visible"))
			return;

		$(".details-popup.visible").removeClass("visible");
		$(".marriage-popup.visible").removeClass("visible");

		$(this).prev(".details-popup").addClass("visible");
		$(document.body).addClass("popup-shown");
	});

	function _toggleImageSize(img)
	{
		var anchor = img.parent();
		var imgUrl = img.attr("src");
		var anchorUrl = anchor.attr("href");

		if (imgUrl === anchorUrl && img.data("original-url"))
			img.attr("src", img.data("original-url"));
		else {
			img.data("original-url", imgUrl);
			img.attr("src", anchorUrl);
			img.attr("height", null);
			img.attr("width", null);
		}

		return false;
	}

	$(".person .details-popup").click(function (event) {
		var target = $(event.target);

		if (event.target.tagName === "IMG") {
			return _toggleImageSize(target);
		}

		$(this).removeClass("visible");
		$(document.body).removeClass("popup-shown");
		return true;
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

	$("img").unveil(0, function () {
	    console.log("Unveiled '" + $(this).attr("src") + "'");
	});
});