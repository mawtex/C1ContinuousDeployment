<?xml version="1.0" encoding="UTF-8" ?>
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="compare.aspx.cs" Inherits="Composite_InstalledPackages_content_views_ContentVersioning_compare" %>
<html xmlns="http://www.w3.org/1999/xhtml" xmlns:ui="http://www.w3.org/1999/xhtml" xmlns:control="http://www.composite.net/ns/uicontrol">
	<control:httpheaders runat="server" />
	<head>
		<title>Compare versions</title>
		<control:styleloader runat="server" />
		<control:scriptloader type="sub" runat="server" />
		<script type="text/javascript" src="bindings/ComparePageBinding.js"></script>
		<link rel="stylesheet" type="text/css" href="compare.css.aspx" />
		<link rel="stylesheet" type="text/css" href="print.css.aspx" media="print" />  
	</head>
	<body>
		<form id="Form1" runat="server" >
		<ui:page id="page" label="Compare versions" image="${icon:versioning-compare}" binding="ComparePageBinding">
			<ui:toolbar id="toolbar" >
				<ui:toolbarbody>
					<ui:toolbargroup>
						<ui:toolbarbutton id="exportbutton"  label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.Export}"  callbackid="export" image="${icon:mimetype-xls}" />
					</ui:toolbargroup>
					<ui:toolbargroup>
						<ui:toolbarbutton id="printbutton" label="${string:FunctionDocumentation.LabelButtonPrint}"
							oncommand="bindingMap.page.print()" image="${icon:print}"
							tooltip="${string:FunctionDocumentation.LabelButtonPrint}" />
					</ui:toolbargroup>
				</ui:toolbarbody>
			</ui:toolbar>
			<ui:scrollbox id="scrollbox">
				<asp:PlaceHolder ID="Holder" runat="server" />
			</ui:scrollbox>
		</ui:page>
			</form>
	</body>
</html>