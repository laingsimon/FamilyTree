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

	$(".marriage").click(function (event)
	{
		event.stopPropagation();

		if (!closeAnyVisibleParticulars())
		{
			alert("not implemented");
		}

		return false;
	});

	function closeAnyVisibleParticulars()
	{
		if (!$(document.body).hasClass("view-full-details"))
			return false;

		var visibleFullDetails = $(".particulars-container.full-details.visible");
		toggleFullDetails(visibleFullDetails);
		return true;
	}

	$(document.body).click(function (event)
	{
		closeAnyVisibleParticulars();
	});
});