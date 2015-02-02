<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">

	<xsl:template name="edit">
		<link type="text/css" rel="stylesheet" href="../../Content/edit.css" />
		<script src="../../Scripts/edit.js"></script>

		<div id="editMember">
			<h2>Edit Member</h2>
			<fieldset>
				<legend>Names</legend>
				<label>
					First
					<input name="FirstName" />
				</label>
				<label>
					Middle
					<input name="MiddleName" />
				</label>
				<label>
					Last
					<input name="LastName" />
				</label>
				<label>
					Nick
					<input name="NickName" />
				</label>
			</fieldset>
			<fieldset>
				<legend>Dates</legend>
				<label>
					Birth
					<input name="DateOfBirth" />
				</label>
			</fieldset>

			<button data-button-type="submit" type="submit">Apply changes</button>
			<button data-button-type="cancel">Cancel</button>
		</div>
	</xsl:template>
</xsl:stylesheet>
