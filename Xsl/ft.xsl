<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
 <xsl:include href="pop-up.xsl" />
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

    <xsl:call-template name="pop-up" />
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
        <xsl:text> repeated</xsl:text> <!-- person had children out of marriage, or is a subsequent marriage -->
       </xsl:if>
      </xsl:attribute>
      <div class="child-connector">
       <div class="vertical"></div>
      </div>
      <xsl:apply-templates mode="Particulars" select=".." />
     </div>
    <div class="symbol">
     <xsl:apply-templates mode="Symbol" select="." />
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
     <xsl:value-of select="translate(Children/@SeeOtherTree, ' ',  '-')" />
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

 <xsl:template mode="Symbol" match="Marriage">
  <xsl:choose>
   <xsl:when test="@Status = 'Divorced'">
    <xsl:text>#</xsl:text>
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

 <xsl:template match="Person" mode="Particulars">
  <xsl:variable name="fullName">
   <xsl:apply-templates select="Name" mode="FullName" />
  </xsl:variable>

  <xsl:variable name="deadClass">
   <xsl:if test="Death/@Date">
    <xsl:text> dead</xsl:text>
   </xsl:if>
  </xsl:variable>

  <xsl:variable name="path">
   <xsl:text>~/Data/</xsl:text>
   <xsl:value-of select="Name/@Last" />
   <xsl:text>.xml</xsl:text>
  </xsl:variable>

  <xsl:variable name="handle">
   <xsl:apply-templates select="." mode="Handle" />
  </xsl:variable>

  <xsl:variable name="photo">
   <xsl:text>../../Photo/Index/</xsl:text>
		<xsl:value-of select="Name/@Last"/>
		<xsl:text>/</xsl:text>
		<xsl:value-of select="Name/@First" />
		<xsl:text>/</xsl:text>
		<xsl:choose>
			<xsl:when test="Name/@Middle">
				<xsl:value-of select="Name/@Middle" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>-</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text>/</xsl:text>

		<xsl:choose>
			<xsl:when test="Birth/@Date and Birth/@Date != ''">
				<xsl:value-of select="translate(Birth/@Date, '/', '-')" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>01-01-0001</xsl:text>
			</xsl:otherwise>
		</xsl:choose>

		<xsl:text>/h50</xsl:text>
  </xsl:variable>

  <div class="particulars-container">
   <a name="{$handle}" class="handle"></a>
    <div class="{@Gender}{$deadClass} particulars" fullName="{$fullName}" style="background-image: url('{translate($photo, '?', '')}')">
     <xsl:attribute name="tree-available">
      <xsl:choose>
       <xsl:when test="count(document($path)/Tree) > 0">
        <xsl:text>true</xsl:text>
       </xsl:when>
      </xsl:choose>
     </xsl:attribute>

     <span class="first-name"><xsl:value-of select="Name/@First" /></span>
     <span class="last-name"><xsl:value-of select="Name/@Last" /></span>
     <span class="date-of-birth"><xsl:value-of select="Birth/@Date" /><br /></span>
     <span class="date-of-death hidden"><xsl:value-of select="Death/@Date" /><br /></span>
    </div>
   </div>
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
