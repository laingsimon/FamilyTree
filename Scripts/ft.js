$(document).ready(function ()
{
	var highlight = window.location.hash;
	if (highlight)
	{
		var highlightElement = $("a[name = '" + highlight.replace("#", "") + "']");

		if (highlightElement.length > 0)
		{
			highlightElement.next()[0].scrollIntoView();
			highlightElement.next().addClass("highlight");
		}
		else
		{
			alert("Cannot find person to highlight " + highlight);
		}
	}

	$(".particulars").click(function (event)
	{
		if ($(event.target).parent().hasClass("operations"))
		{
			event.stopPropagation();
			return;
		}

		if (!closeAnyVisibleParticulars())
			toggleFullDetails($(this));

		event.stopPropagation();
		return false;
	});

	$(".particulars img").click(function (event) {
		event.stopPropagation();

		var url = $(this).attr("src");
		url = url.replace(/h\d+$/g, "");

		window.open(url, "full-photo", "", true);

		return false;
	});

	$(".marriage img").click(function (event) {
		event.stopPropagation();

		var url = $(this).attr("src");
		url = url.replace(/h\d+$/g, "");

		window.open(url, "full-photo", "", true);

		return false;
	});

	function toggleFullDetails(element)
	{
		var isFullDetails = element.closest(".particulars-container").hasClass("full-details");
		var summaryDetails = isFullDetails ? element.closest(".particulars-container").next(".summary-details") : element.closest(".particulars-container");

		if (isFullDetails)
		{
			summaryDetails.css("visibility", "");
			element.closest(".particulars-container").removeClass("visible");
			$(document.body).removeClass("view-full-details");
		}
		else
		{
			var fullDetails = element.closest(".particulars-container").prev(".full-details");

			fullDetails.css("left", (summaryDetails.width() / 2) + "px");
			fullDetails.css("width", summaryDetails.width() + "px");
			summaryDetails.css("visibility", "hidden");
			fullDetails.addClass("visible");
			$(document.body).addClass("view-full-details");
		}
	}

	$(".marriage .symbol").click(function (event)
	{
		event.stopPropagation();

		if ($(event.target).parent().hasClass("operations"))
			return;

		if ($(event.target).parent().hasClass("marriage-to"))
			return;

		if (!closeAnyVisibleParticulars())
			toggleMarriageDetails($(this));

		return false;
	});

	function toggleMarriageDetails(element)
	{
		var details = $(element).closest(".symbol").find(".marriage-details");

		if (details.hasClass("visible"))
		{
			$(document.body).removeClass("view-full-details");
			details.removeClass("visible");
		}
		else
		{
			$(document.body).addClass("view-full-details");
			details.addClass("visible");
		}
	}

	function closeAnyVisibleParticulars()
	{
		if (!$(document.body).hasClass("view-full-details"))
			return false;

		var visibleFullDetails = $(".particulars-container.full-details.visible");
		if (visibleFullDetails.length > 0)
			toggleFullDetails(visibleFullDetails);

		var visibleMarriageDetails = $(document.body).find(".marriage-details.visible");
		if (visibleMarriageDetails.length > 0)
			toggleMarriageDetails(visibleMarriageDetails);

		return true;
	}

	$(document.body).click(function (event)
	{
		closeAnyVisibleParticulars();
	});
});