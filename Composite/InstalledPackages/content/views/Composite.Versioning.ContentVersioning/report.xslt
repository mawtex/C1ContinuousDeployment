<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
	exclude-result-prefixes="msxsl" 
	xmlns="http://www.w3.org/1999/xhtml"  
	xmlns:ui="http://www.w3.org/1999/xhtml">

	<xsl:template match="/">
		<table id="versions" binding="ReportTableBinding" contextmenu="contextmenu">
			<tbody>
				<xsl:for-each select="/Report/Task">
					<xsl:choose>
						<xsl:when test="count(Activity)">
							<xsl:variable name="username" select="@Username" />
							<xsl:variable name="tasktype" select="@TaskType" />
							<xsl:variable name="tasklabel" select="@Label" />
							
							<xsl:for-each select="Activity">
								<tr
									viewaction="{@ShowAction}"
									viewfileaction="{@ShowFileAction}"
									compareaction="alert('TODO!')"
									restoreaction="{@RollbackAction}"
									comparetoken="{@CompareEntityToken}"
									comparetotoken="{@CompareToActionToken}"
									piggybag="{@Piggybag}"
									piggybagHash="{@PiggybagHash}">
									<td class="time">
										<xsl:value-of select="@ActivityTime" />
									</td>
									<td class="user">
										<ui:labelbox label="{$username}">
											<xsl:attribute name="image">${icon:user}</xsl:attribute>
										</ui:labelbox>
									</td>
									<td class="task">
										<ui:labelbox label="{$tasklabel}" imagekey="{$tasktype}" versioningrole="task" binding="ReportLabelBinding"/>
									</td>
									<td class="activity">
										<ui:labelbox label="{@Label}" imagekey="{@ActivityType}" versioningrole="activity" binding="ReportLabelBinding"/>
									</td>
									<td class="comment"/>
								</tr>
							</xsl:for-each>
						</xsl:when>
						<xsl:otherwise>
							<tr>
								<td class="time">
									<xsl:value-of select="@StartTime" />
								</td>
								<td class="user">
									<ui:labelbox label="{@Username}">
										<xsl:attribute name="image">${icon:user}</xsl:attribute>
									</ui:labelbox>
								</td>
								<td class="task">
									<ui:labelbox label="{@Label}" imagekey="{@TaskType}" versioningrole="task" binding="ReportLabelBinding"/>
								</td>
								<td class="activity"/>
								<td class="comment"/>
							</tr>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:for-each>
			</tbody>
		</table>
	</xsl:template>
	
</xsl:stylesheet>