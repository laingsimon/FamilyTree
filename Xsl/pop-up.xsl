<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:template name="pop-up">
		<link rel="stylesheet" href="../../Content/pop-up.css" />
		<script src="../../Scripts/pop-up.js"></script>

		<div class="pop-up-container" style="display: none;">
			<div class="pop-up-background"></div>
			<div class="pop-up">
				<h2></h2>
				<div class="scrolling-content"></div>
				<div class="buttons">
					<input name="ok" type="button" value="OK" />
					<input name="cancel" type="button" value="Cancel" />
				</div>
			</div>
		</div>
	</xsl:template>

</xsl:stylesheet>
