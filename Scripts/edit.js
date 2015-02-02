$(document).ready(function ()
{
	var editMemberElement = $("#editMember");
	var context = null;

	window.editMember = function (context)
	{
		_context = context;

		_setData(_context);

		editMemberElement.show();
		editMemberElement.css("left", (($(window).width() / 2) - (editMemberElement.width() / 2)) + "px");
		editMemberElement.css("top", (($(window).height() / 2) - (editMemberElement.height() / 2)) + "px");
	};

	function _setData(context)
	{
		editMemberElement.find("input[name='FirstName']").val(context.data("first-name"));
		editMemberElement.find("input[name='LastName']").val(context.data("last-name"));
		editMemberElement.find("input[name='MiddleName']").val(context.data("middle-name"));
		editMemberElement.find("input[name='NickName']").val(context.data("nick-name"));
		editMemberElement.find("input[name='DateOfBirth']").val(context.data("date-of-birth"));
	}

	$("#editMember button[data-button-type='cancel']").click(function ()
	{
		editMemberElement.hide();
	});

	$("#editMember button[data-button-type='submit']").click(function ()
	{
		var family = _context.data("last-name");
		var path = encodeURI(_context.data("xpath"));

		var form = $("<form target='_blank' action=\"/FamilyTree/Person/Update/" + family + "/?path=" + path + "\" method='post' />");
		form.html(editMemberElement.html());

		editMemberElement.find("input[name]").each(function ()
		{
			var formElement = form.find("input[name='" + $(this).attr("name") + "']");
			formElement.val($(this).val());
		});

		form.submit();
	});
});