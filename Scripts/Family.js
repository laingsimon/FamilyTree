﻿$(document).ready(
	function () {
		$(".person .details").click(function () {
			$(".details-popup.visible").removeClass("visible");

			$(this).prev(".details-popup").addClass("visible");
			$(document.body).addClass("popup-shown");
		});

		$(".person .details-popup").click(function () {
			$(this).removeClass("visible");
			$(document.body).removeClass("popup-shown");
		});
});