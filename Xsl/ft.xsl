<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" omit-xml-declaration="yes" />

	<xsl:template match="Tree">
		<html>
			<head>
				<meta name="viewport" content="width=device-width, initial-scale=0.6" />

				<title>
					<xsl:value-of select="//Tree/@Family" />
					<xsl:text> Family Tree</xsl:text>
				</title>
				<link type="text/css" rel="stylesheet" href="../../Content/ft.css" />
				<script type="text/javascript" lanugage="javascript" src="../../Scripts/jquery-1.6.4.js"></script>
				<script type="text/javascript" lanugage="javascript" src="../../Scripts/ft.js"></script>
			</head>
			<body family="{//Tree/@Family}">
				<table cellspacing="0" cellpadding="0">
					<tr>
						<xsl:apply-templates select="Person" />
					</tr>
				</table>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="Person">
		<xsl:param name="siblings" />

		<xsl:if test="count(Children) > 0">
			<xsl:apply-templates select="." mode="OwnChildren" >
				<xsl:with-param name="position" select="position()" />
			</xsl:apply-templates>
		</xsl:if>
		<xsl:if test="count(Marriage) > 0">
			<xsl:apply-templates mode="Marriage" select="Marriage">
				<xsl:with-param name="position" select="position()" />
			</xsl:apply-templates>
		</xsl:if>
		<xsl:if test="count(Children) = 0 and count(Marriage) = 0">
			<td valign="top" type="child-details">
				<xsl:if test="$siblings = 1">
					<xsl:attribute name="class">
						<xsl:text>only-child</xsl:text>
					</xsl:attribute>
				</xsl:if>

				<div class="child-connector">
					<div class="vertical"></div>
				</div>
				<div class="person">
					<xsl:apply-templates mode="Particulars" select="." />
				</div>
			</td>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Marriage" mode="Marriage">
		<xsl:param name="position" />
		<xsl:variable name="onlyChild" select="count(../Marriage) = 1 and count(../../Person) = 1" />

		<td valign="top" type="married-children" count="{count(Children/Person)}" marriages="{count(../../Person)}">
			<xsl:if test="$onlyChild">
				<xsl:attribute name="class">
					<xsl:text>only-child</xsl:text>
				</xsl:attribute>
			</xsl:if>

			<div class="mid-line"></div>

			<div class="marriage" date="{@Date}" children="{count(Children/Person)}">
				<div class="people">
					<div>
						<xsl:attribute name="class">
							<xsl:text>person</xsl:text>
							<xsl:if test="count(../Children) > 0 or position() > 1">
								<xsl:text> repeated</xsl:text>
								<!-- person had children out of marriage, or is a subsequent marriage -->
							</xsl:if>
						</xsl:attribute>
						<div class="child-connector">
							<div class="vertical"></div>
						</div>
						<xsl:apply-templates mode="Particulars" select=".." />
					</div>
					<div class="symbol">
						<xsl:apply-templates mode="Symbol" select="." />
						<xsl:apply-templates mode="MarriageDetails" select="." />
					</div>
					<div class="person">
						<xsl:variable name="allMarriages">
							<xsl:for-each select="../Marriage">
								<marriage>
									<to>
										<xsl:apply-templates mode="Handle" select="To/Person" />
									</to>
								</marriage>
							</xsl:for-each>
						</xsl:variable>
						<xsl:variable name="myHandle">
							<xsl:apply-templates mode="Handle" select=".." />
						</xsl:variable>
						<!-- /marriage/to[.=$myHandle] 
      <xsl:value-of select="count(exsl:node-set($allMarriages))" /> -->
						<xsl:apply-templates mode="Particulars" select="To/Person" />
					</div>
				</div>
				<xsl:if test="count(Children/Person) > 0">
					<xsl:apply-templates mode="ShowChildren" select="." />
				</xsl:if>
				<xsl:if test="Children/@SeeOtherTree">
					<xsl:variable name="entryPoint">
						<xsl:apply-templates mode="Handle" select=".." />
						<xsl:text>+</xsl:text>
						<xsl:apply-templates mode="Handle" select="To/Person" />
					</xsl:variable>
					<xsl:variable name="entryPointReversed">
						<xsl:apply-templates mode="Handle" select="To/Person" />
						<xsl:text>+</xsl:text>
						<xsl:apply-templates mode="Handle" select=".." />
					</xsl:variable>
					<xsl:variable name="path">
						<xsl:text>~/Data/</xsl:text>
						<xsl:value-of select="translate(Children/@SeeOtherTree, ' ',  ' ')" />
						<xsl:text>.xml</xsl:text>
					</xsl:variable>
					<xsl:choose>
						<xsl:when test="count(document($path)//Children[@EntryPoint = $entryPoint]) > 0">
							<xsl:apply-templates mode="ShowChildren" select="document($path)//Children[@EntryPoint = $entryPoint]/.." />
						</xsl:when>
						<xsl:when test="count(document($path)//Children[@EntryPoint = $entryPointReversed]) > 0">
							<xsl:apply-templates mode="ShowChildren" select="document($path)//Children[@EntryPoint = $entryPointReversed]/.." />
						</xsl:when>
						<xsl:otherwise>
							<div class="hint">
								<xsl:text>add EntryPoint="</xsl:text>
								<xsl:value-of select="$entryPoint" />
								<xsl:text>" or "</xsl:text>
								<xsl:value-of select="$entryPointReversed" />
								<xsl:text>" to a 'Children' node in </xsl:text>
								<xsl:value-of select="$path" />
								<xsl:text> file</xsl:text>
							</div>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
			</div>
		</td>
	</xsl:template>

	<xsl:template mode="MarriageDetails" match="Marriage">
		<xsl:variable name="marriageFromPhoto">
			<xsl:apply-templates mode="Photo" select=".." />
			<xsl:text>/h75</xsl:text>
		</xsl:variable>

		<xsl:variable name="marriageToPhoto">
			<xsl:apply-templates mode="Photo" select="To/Person" />
			<xsl:text>/h75</xsl:text>
		</xsl:variable>

		<xsl:variable name="marriageToTreeAvailable">
			<xsl:variable name="path">
				<xsl:text>~/Data/</xsl:text>
				<xsl:value-of select="To/Person/Name/@Last" />
				<xsl:text>.xml</xsl:text>
			</xsl:variable>
			<xsl:value-of select="count(document($path)/Tree) > 0" />
		</xsl:variable>

		<xsl:variable name="marriageToLink">
			<xsl:choose>
				<xsl:when test="$marriageToTreeAvailable = 'true'">
					<xsl:text>../../Tree/Family/</xsl:text>
					<xsl:value-of select="To/Person/Name/@Last" />
					<xsl:text>#</xsl:text>
					<xsl:apply-templates select="To/Person" mode="Handle" />
				</xsl:when>
				<xsl:otherwise />
			</xsl:choose>
		</xsl:variable>

		<div class="marriage-details">
			<img src="{$marriageFromPhoto}" class="marriage-from"/>
			<img src="{$marriageToPhoto}" class="marriage-to" />
			<span>
				<xsl:text>Marriage of </xsl:text>
				<xsl:value-of select="../Name/@First"/>
				<xsl:text> and </xsl:text>
				<xsl:value-of select="To/Person/Name/@First"/>
				<xsl:if test="@Date != ''">
					<br />
					<xsl:text>on </xsl:text>
					<xsl:value-of select="@Date" />
				</xsl:if>
				<xsl:if test="@Location != ''">
					<br />
					<xsl:text>at </xsl:text>
					<xsl:value-of select="@Location" />
				</xsl:if>
				<xsl:if test="@Status != ''">
					<br />
					<xsl:text> now </xsl:text>
					<xsl:value-of select="@Status" />
				</xsl:if>
			</span>

			<xsl:if test="$marriageToTreeAvailable = 'true'">
				<div class="operations">
					<a href="{$marriageToLink}" class="open-tree">
						<xsl:text>Open </xsl:text>
						<xsl:value-of select="To/Person/Name/@Last"/>
						<xsl:text> tree</xsl:text>
					</a>
				</div>
			</xsl:if>
		</div>
	</xsl:template>

	<xsl:template mode="Symbol" match="Marriage">
		<xsl:choose>
			<xsl:when test="@Status = 'Divorced'">
				<xsl:text>&#8800;</xsl:text>
			</xsl:when>
			<xsl:when test="@Type = 'CommonLaw'">
				<xsl:text>-</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>=</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="Person" mode="OwnChildren">
		<xsl:param name="position" />

		<td valign="top" type="own-children" count="{count(Children/Person)}">
			<xsl:if test="count(Children/Person) = 1">
				<xsl:attribute name="class">
					<xsl:text>only-child</xsl:text>
				</xsl:attribute>
			</xsl:if>

			<div class="child-connector">
				<div class="vertical"></div>
			</div>
			<div class="person">
				<xsl:apply-templates mode="Particulars" select="." />
				<xsl:apply-templates mode="ShowChildren" select="." />
			</div>
		</td>
	</xsl:template>

	<xsl:template mode="ShowChildren" match="*">
		<xsl:if test="count(Children/Person) > 0">
			<div class="children">
				<div class="connector"></div>
				<table cellspacing="0" cellpadding="0">
					<tr name="{name(.)}" count="{count(Children)}">
						<xsl:apply-templates select="Children/Person">
							<xsl:with-param name="siblings" select="count(Children/Person)" />
						</xsl:apply-templates>
					</tr>
				</table>
			</div>
		</xsl:if>
	</xsl:template>

	<xsl:template mode="Photo" match="Person">
		<xsl:text>../../Photo/Index/</xsl:text>
		<xsl:value-of select="translate(Name/@Last, '?', '-')"/>
		<xsl:text>/</xsl:text>
		<xsl:value-of select="translate(Name/@First, '?', '-')" />
		<xsl:text>/</xsl:text>
		<xsl:choose>
			<xsl:when test="Name/@Middle and Name/@Middle != ''">
				<xsl:value-of select="Name/@Middle" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>-</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text>/</xsl:text>

		<xsl:choose>
			<xsl:when test="Birth/@Date and Birth/@Date != '' and Birth/@Date != '?'">
				<xsl:value-of select="translate(translate(Birth/@Date, '/', '-'), '?', '0')" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>01-01-0001</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="Person" mode="Particulars">
		<xsl:variable name="fullName">
			<xsl:apply-templates select="Name" mode="FullName" />
		</xsl:variable>

		<xsl:variable name="handle">
			<xsl:apply-templates select="." mode="Handle" />
		</xsl:variable>

		<xsl:variable name="photo">
			<xsl:apply-templates mode="Photo" select="." />
		</xsl:variable>

		<xsl:variable name="known-name">
			<xsl:choose>
				<xsl:when test="Name/@Nickname != ''">
					<xsl:text>'</xsl:text>
					<xsl:value-of select="Name/@Nickname" />
					<xsl:text>'</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="Name/@First" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:apply-templates mode="FullDetails" select=".">
			<xsl:with-param name="known-name" select="$known-name" />
			<xsl:with-param name="handle" select="$handle" />
			<xsl:with-param name="photo" select="$photo" />
		</xsl:apply-templates>
		<xsl:apply-templates mode="SummaryDetails" select=".">
			<xsl:with-param name="known-name" select="$known-name" />
			<xsl:with-param name="handle" select="$handle" />
			<xsl:with-param name="photo" select="$photo" />
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template mode="SummaryDetails" match="Person">
		<xsl:param name="known-name" />
		<xsl:param name="handle" />
		<xsl:param name="photo" />

		<xsl:variable name="deadClass">
			<xsl:if test="Death/@Date">
				<xsl:text> dead</xsl:text>
			</xsl:if>
		</xsl:variable>

		<div class="particulars-container summary-details">
			<a name="{$handle}" class="handle"></a>
			<div class="{@Gender}{$deadClass} particulars" style="background-image: url('{translate($photo, '?', '')}/h50')">
				<span class="first-name">
					<xsl:value-of select="$known-name" />
				</span>
				<span class="last-name">
					<xsl:value-of select="Name/@Last" />
				</span>
				<span class="date-of-birth">
					<xsl:value-of select="Birth/@Date" />
					<br />
				</span>
			</div>
		</div>
	</xsl:template>

	<xsl:template mode="FullDetails" match="Person">
		<xsl:param name="known-name" />
		<xsl:param name="handle" />
		<xsl:param name="photo" />

		<!-- not currently used -->
		<xsl:variable name="xpath">
			<xsl:apply-templates mode="XPath" select="." />
		</xsl:variable>

		<xsl:variable name="path">
			<xsl:text>~/Data/</xsl:text>
			<xsl:value-of select="Name/@Last" />
			<xsl:text>.xml</xsl:text>
		</xsl:variable>

		<div class="particulars-container full-details">
			<div class="{@Gender} particulars">
				<table>
					<tr>
						<td>
							<img src="{translate($photo, '?', '')}/h75" height="75px" style="background-image: url('{translate($photo, '?', '')}/h50')" />
						</td>
						<td>
							<label>
								<xsl:choose>
									<xsl:when test="Name/@Nickname != ''">
										<xsl:text>'</xsl:text>
										<xsl:value-of select="Name/@Nickname"/>
										<xsl:text>'</xsl:text>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="Name/@First"/>
									</xsl:otherwise>
								</xsl:choose>
								<xsl:text> </xsl:text>
								<xsl:value-of select="Name/@Last"/>
							</label>
							<xsl:if test="Name/@Nickname != ''">
								<xsl:text>First name: </xsl:text>
								<xsl:value-of select="Name/@First"/>
							</xsl:if>

							<xsl:if test="Name/@Middle != ''">
								<br />
								<xsl:text>Other name(s): </xsl:text>
								<xsl:value-of select="Name/@Middle"/>
							</xsl:if>
							<xsl:if test="Birth/@Date != ''">
								<br />
								<xsl:text>Born: </xsl:text>
								<xsl:value-of select="Birth/@Date"/>
							</xsl:if>
							<xsl:if test="Death/@Date != '' and Death/@Date != '?'">
								<br />
								<xsl:text>Died: </xsl:text>
								<xsl:value-of select="Death/@Date"/>
							</xsl:if>
						</td>
					</tr>
				</table>
				<div class="operations">
					<xsl:if test="(count(document($path)/Tree) > 0) and (Name/@Last != /Tree/@Family)">
						<a href="../../Tree/Family/{Name/@Last}#{$handle}">
							<xsl:text>Open </xsl:text>
							<xsl:value-of select="Name/@Last"/>
							<xsl:text> tree</xsl:text>
						</a>
					</xsl:if>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Person" mode="XPath">
		<xsl:text>//Person[Name/@First='</xsl:text>
		<xsl:value-of select="Name/@First" />
		<xsl:text>'</xsl:text>
		<xsl:if test="Name/@Middle">
			<xsl:text> and Name/@Middle='</xsl:text>
			<xsl:value-of select="Name/@Middle" />
			<xsl:text>'</xsl:text>
		</xsl:if>
		<xsl:if test="Name/@Last">
			<xsl:text> and Name/@Last='</xsl:text>
			<xsl:value-of select="Name/@Last" />
			<xsl:text>'</xsl:text>
		</xsl:if>

		<xsl:text>]</xsl:text>
	</xsl:template>

	<xsl:template match="Person" mode="Handle">
		<xsl:variable name="fullName">
			<xsl:apply-templates select="Name" mode="FullName">
				<xsl:with-param name="includeNick" select="false()" />
			</xsl:apply-templates>
		</xsl:variable>

		<xsl:value-of select="translate($fullName, ' ', '-')" />

		<xsl:if test="not(not(Birth/@Date)) and not(Birth/@Date = '')">
			<xsl:text>_</xsl:text>
			<xsl:value-of select="translate(Birth/@Date, '/', '')" />
		</xsl:if>
	</xsl:template>

	<xsl:template match="Name" mode="FullName">
		<xsl:param name="includeNick" select="true()" />

		<xsl:if test="@Title">
			<xsl:value-of select="@Title"/>
			<xsl:text> </xsl:text>
		</xsl:if>
		<xsl:value-of select="@First"/>
		<xsl:if test="$includeNick and @Nickname">
			<xsl:text> '</xsl:text>
			<xsl:value-of select="@Nickname"/>
			<xsl:text>'</xsl:text>
		</xsl:if>
		<xsl:text> </xsl:text>
		<xsl:if test="@Middle">
			<xsl:value-of select="@Middle"/>
			<xsl:text> </xsl:text>
		</xsl:if>
		<xsl:value-of select="@Last"/>
	</xsl:template>
</xsl:stylesheet>
