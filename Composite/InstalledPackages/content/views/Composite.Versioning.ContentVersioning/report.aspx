<?xml version="1.0" encoding="UTF-8" ?>

<%@ Page Language="C#" AutoEventWireup="true" CodeFile="report.aspx.cs" Inherits="Composite_InstalledPackages_content_views_ContentVersioning_report" %>

<html xmlns="http://www.w3.org/1999/xhtml" xmlns:ui="http://www.w3.org/1999/xhtml"
xmlns:control="http://www.composite.net/ns/uicontrol">
<control:httpheaders runat="server" />
<head>
	<title>Composite.Management.VersioningReport</title>
	<control:styleloader runat="server" />
	<control:scriptloader type="sub" runat="server" />
	<link rel="stylesheet" type="text/css" href="report.css.aspx" />
	<link rel="stylesheet" type="text/css" href="print.css.aspx" media="print" /> 
	<script type="text/javascript" src="report.js" />
	<script type="text/javascript" src="bindings/ReportTableBinding.js" />
	<script type="text/javascript" src="bindings/ReportToolBarBinding.js" />
	<script type="text/javascript" src="bindings/ReportLabelBinding.js" />
</head>
<body id="root">
	<form id="Form1" runat="server">
	<ui:broadcasterset>
		<ui:broadcaster id="broadcasterHasSelection" isdisabled="true" />
	</ui:broadcasterset>
	<ui:popupset>
		<ui:popup id="contextmenu">
			<ui:menubody>
				<ui:menugroup>
					<asp:PlaceHolder ID="ViewDataContextMenu" runat="server" Visible="false">
						<ui:menuitem label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.LabelView}"
							image="${icon:versioning-view}" image-disabled="${icon:versioning-view-disabled}"
							tooltip="${string:Composite.Versioning.ContentVersioning:ToolbarButton.ToolTipView}"
							observes="broadcasterHasSelection" oncommand="bindingMap.toolbar.action('view');" />
					</asp:PlaceHolder>
					<asp:PlaceHolder ID="ViewMediaContextMenu" runat="server" Visible="false">
						<ui:menuitem label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.LabelViewSettings}"
							image="${icon:versioning-view}" image-disabled="${icon:versioning-view-disabled}"
							tooltip="${string:Composite.Versioning.ContentVersioning:ToolbarButton.ToolTipViewSettings}"
							observes="broadcasterHasSelection" oncommand="bindingMap.toolbar.action('view');" />
						<ui:menuitem label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.LabelViewFile}"
							image="${icon:versioning-view}" image-disabled="${icon:versioning-view-disabled}"
							tooltip="${string:Composite.Versioning.ContentVersioning:ToolbarButton.ToolTipViewFile}"
							observes="broadcasterHasSelection" oncommand="bindingMap.toolbar.action('viewfile');" />
					</asp:PlaceHolder>
					<ui:menuitem label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.LabelCompare}"
						image="${icon:versioning-compare}" image-disabled="${icon:versioning-compare-disabled}"
						tooltip="${string:Composite.Versioning.ContentVersioning:ToolbarButton.ToolTipCompare}"
						observes="broadcasterHasSelection" oncommand="bindingMap.toolbar.action('compare');" />
					<ui:menuitem label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.LabelRestore}"
						image="${icon:versioning-restore}" image-disabled="${icon:versioning-restore-disabled}"
						tooltip="${string:Composite.Versioning.ContentVersioning:ToolbarButton.ToolTipRestore}"
						observes="broadcasterHasSelection" oncommand="bindingMap.toolbar.action('restore');" />
				</ui:menugroup>
			</ui:menubody>
		</ui:popup>
	</ui:popupset>
	<ui:cursor id="comparecursor" image="${icon:versioning-compare}" />
	<ui:page id="page" image="${icon:report}">
		<ui:toolbar id="toolbar" binding="ReportToolBarBinding">
			<ui:toolbarbody>
				<ui:toolbargroup>
					<aspui:ToolbarButton ID="ToolBarButton1" AutoPostBack="true" 
						Text="${string:Composite.Versioning.ContentVersioning:ToolbarButton.LabelRefresh}" 
						ImageUrl="${icon:refresh}"
						runat="server" OnClick="ContentChanged" />
				</ui:toolbargroup>
				<ui:toolbargroup>
					<asp:PlaceHolder ID="ViewDataToolbar" runat="server" Visible="false">
						<ui:toolbarbutton label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.LabelView}"
							cmd="view" image="${icon:versioning-view}" image-disabled="${icon:versioning-view-disabled}"
							tooltip="${string:Composite.Versioning.ContentVersioning:ToolbarButton.ToolTipView}"
							observes="broadcasterHasSelection" />
					</asp:PlaceHolder>
					<asp:PlaceHolder ID="ViewMediaToolbar" runat="server" Visible="false">
						<ui:toolbarbutton label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.LabelViewSettings}"
							cmd="view" image="${icon:versioning-view}" image-disabled="${icon:versioning-view-disabled}"
							tooltip="${string:Composite.Versioning.ContentVersioning:ToolbarButton.ToolTipViewSettings}"
							observes="broadcasterHasSelection" />
						<ui:toolbarbutton label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.LabelViewFile}"
							cmd="viewfile" image="${icon:versioning-view}" image-disabled="${icon:versioning-view-disabled}"
							tooltip="${string:Composite.Versioning.ContentVersioning:ToolbarButton.ToolTipViewFile}"
							observes="broadcasterHasSelection" />
					</asp:PlaceHolder>
				</ui:toolbargroup>
				<ui:toolbargroup>
					<ui:toolbarbutton label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.LabelCompare}"
						cmd="compare" image="${icon:versioning-compare}" image-disabled="${icon:versioning-compare-disabled}"
						tooltip="${string:Composite.Versioning.ContentVersioning:ToolbarButton.ToolTipCompare}"
						observes="broadcasterHasSelection" />
				</ui:toolbargroup>
				<ui:toolbargroup>
					<ui:toolbarbutton label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.LabelRestore}"
						cmd="restore" image="${icon:versioning-restore}" image-disabled="${icon:versioning-restore-disabled}"
						tooltip="${string:Composite.Versioning.ContentVersioning:ToolbarButton.ToolTipRestore}"
						observes="broadcasterHasSelection" />
				</ui:toolbargroup>
				<ui:toolbargroup>
					<aspui:Selector runat="server" ID="Pager" AutoPostBack="true" OnSelectedIndexChanged="ContentChanged">
						<asp:ListItem>10</asp:ListItem>
						<asp:ListItem>25</asp:ListItem>
						<asp:ListItem>50</asp:ListItem>
						<asp:ListItem>100</asp:ListItem>
						<asp:ListItem Text="${string:Composite.Versioning.ContentVersioning:Toolbar.RowsToShow.LabelAll}" Value="1000" />
					</aspui:Selector>
				</ui:toolbargroup>
				<ui:toolbargroup>
						<ui:toolbarbutton id="exportbutton" label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.Export}" cmd="export" image="${icon:mimetype-xls}" />
					</ui:toolbargroup>
					<ui:toolbargroup>
						<ui:toolbarbutton id="printbutton" label="${string:FunctionDocumentation.LabelButtonPrint}"
							oncommand="print()" image="${icon:print}"
							tooltip="${string:FunctionDocumentation.LabelButtonPrint}" />
					</ui:toolbargroup>
			</ui:toolbarbody>
		</ui:toolbar>
		<table id="headings" width="100%">
			<thead>
				<tr>
					<th class="time">
						<ui:text label="${string:Composite.Versioning.ContentVersioning:Table.HeadingTime}" />
					</th>
					<th class="user">
						<ui:text label="${string:Composite.Versioning.ContentVersioning:Table.HeadingUser}" />
					</th>
					<th class="task">
						<ui:text label="${string:Composite.Versioning.ContentVersioning:Table.HeadingTask}" />
					</th>
					<th class="activity">
						<ui:text label="${string:Composite.Versioning.ContentVersioning:Table.HeadingActivity}" />
					</th>
					<th class="comment" />
				</tr>
			</thead>
		</table>
		<ui:flexbox id="flexbox">
			<ui:scrollbox id="scrollbox">
				<div id="ContentUpdatePanel">
					<asp:PlaceHolder ID="Holder" runat="server" />
				</div>
			</ui:scrollbox>
			<ui:cover id="cover" hidden="true" />
		</ui:flexbox>
	</ui:page>
	</form>
</body>
</html>
