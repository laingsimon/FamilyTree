$(document).ready(
	function () {
		$(".person .details").click(function () {
			$(this).prev(".details-popup").css("display", "flex");
		});

		$(".person .details-popup").click(function () {
			$(this).hide();
		});
});