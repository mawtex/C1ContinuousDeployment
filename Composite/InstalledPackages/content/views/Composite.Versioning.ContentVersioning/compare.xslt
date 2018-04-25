<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet 
version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
	xmlns="http://www.w3.org/1999/xhtml"
	xmlns:ui="http://www.w3.org/1999/xhtml" 
	xmlns:csharp="http://c1.composite.net/sample/csharp"
	exclude-result-prefixes="msxsl csharp">
	
	<msxsl:script implements-prefix="csharp" language="C#">
	<msxsl:assembly name="System.Xml.Linq" />
	<msxsl:using namespace="System.Xml.Linq" />
	<msxsl:using namespace="System.Xml" />
	<msxsl:using namespace="System.Xml.XPath" /> 
		public string GetAsString(XPathNavigator nav)
		{
			return XDocument.Parse( nav.OuterXml ).ToString();
		}
	</msxsl:script> 

	<xsl:template match="/">
	
		<div>
		
			<xsl:for-each select="/CompareReport/Data">
			
				<table class="compare">
					<caption>
						<xsl:value-of select="@caption" />
					</caption>
					<tr>
						<th class="prop">Property</th>
						<th class="value">Value</th>
					</tr>
					<xsl:for-each select="Property">
					
						<xsl:variable name="new" select="Compare/text()"/>
						<xsl:variable name="old" select="CompareTo/text()"/>
					
						<tr>
							<xsl:if test="@Equals='false'">
								<xsl:attribute name="class">diff</xsl:attribute>
							</xsl:if>
							<td class="prop">
								<ui:labelbox label="{@Label}">
									<xsl:attribute name="image">
										<xsl:choose>
											<xsl:when test="@Equals='false'">${icon:parameter_overloaded}</xsl:when>
											<xsl:otherwise>${icon:blank}</xsl:otherwise>
										</xsl:choose>
									</xsl:attribute>
								</ui:labelbox>
							</td>
							<xsl:choose>
								<xsl:when test="@IsMarkup='true'">
									<td class="value">
										<xsl:choose>
											<xsl:when test="$old=$new">
												<ui:labelbox label="[No difference]"/>
											</xsl:when>
											<xsl:otherwise>
												<a href="javascript:void(false);" class="compare" onclick="bindingMap.page.compare ( this )">
													<xsl:attribute name="newval">
														<xsl:value-of select="Compare"/>
													</xsl:attribute>
													<xsl:attribute name="oldval">
														<xsl:value-of select="CompareTo"/>
													</xsl:attribute>
													<xsl:text>Compare</xsl:text>
												</a>
											</xsl:otherwise>
										</xsl:choose>
									</td>
								</xsl:when>
								<xsl:otherwise>
									<td class="value">
										<xsl:if test="$new != '' or $old!=''">
											<xsl:choose>
												<xsl:when test="$new = $old">
													<xsl:value-of select="$new"/>
												</xsl:when>
												<xsl:otherwise>
													<strong>
														<xsl:value-of select="$new"/>
													</strong>
													<xsl:if test="$old != '' and $new != ''">
														<xsl:text>&#160;/&#160;</xsl:text>
													</xsl:if>
													<strike>
														<xsl:value-of select="$old"/>
													</strike>
												</xsl:otherwise>
											</xsl:choose>
										</xsl:if>
									</td>
								</xsl:otherwise>
							</xsl:choose>
						</tr>
					</xsl:for-each>
					
				</table>
				
			</xsl:for-each>
		
		</div>
		
	</xsl:template>
	
	<xsl:template name="hack">
		<xsl:param name="node"/>
		<xsl:variable name="markup" select="csharp:GetAsString ( $node )"/>
		<xsl:value-of select="$markup"/>
	</xsl:template>
	
</xsl:stylesheet>