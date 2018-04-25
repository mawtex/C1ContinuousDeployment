<?xml version="1.0" encoding="UTF-8"?>
<html xmlns="http://www.w3.org/1999/xhtml" xmlns:ui="http://www.w3.org/1999/xhtml" xmlns:control="http://www.composite.net/ns/uicontrol">
	<control:httpheaders runat="server" />
	<head>
		<title>Composite.Versioning.ContentVersioning.Compare</title>
		<control:styleloader runat="server" />
		<control:scriptloader type="sub" runat="server" />
		<script type="text/javascript" src="CompareDialogPageBinding.js"/>
	</head>
	<body>
		<ui:dialogpage binding="CompareDialogPageBinding"
			label="${string:Composite.Versioning.ContentVersioning:CompareDialog.DialogTitle}" 
			image="${icon:versioning-compare}" 
			height="auto">
			<ui:pagebody>
				<ui:fields>
					<ui:fieldgroup label="${string:Composite.Versioning.ContentVersioning:CompareDialog.FieldGroupLabel}">
						<ui:field>
							<ui:fielddesc label="${string:Composite.Versioning.ContentVersioning:CompareDialog.FieldDescLabel}"/>
							<ui:fielddata>
								<ui:radiodatagroup name="comparison">
									<ui:radio label="${string:Composite.Versioning.ContentVersioning:CompareDialog.OptionOne}" value="current" id="currentoption"/>
									<ui:radio label="${string:Composite.Versioning.ContentVersioning:CompareDialog.OptionTwo}" value="other" relate="other" id="otheroption"/>
								</ui:radiodatagroup>
							</ui:fielddata>
						</ui:field>
						<ui:field relation="other">
							<ui:fielddata>
								<br/>
								<ui:labelbox image="${icon:message}" label="${string:Composite.Versioning.ContentVersioning:CompareDialog.ExitMessage}"/>
							</ui:fielddata>
						</ui:field>
					</ui:fieldgroup>
				</ui:fields>
			</ui:pagebody>
			<ui:dialogtoolbar>
				<ui:toolbarbody align="right" equalsize="true">
					<ui:toolbargroup>
						<ui:clickbutton label="${string:Website.Dialogs.LabelAccept}" id="buttonAccept" response="accept" focusable="true" default="true"/>
						<ui:clickbutton label="${string:Website.Dialogs.LabelCancel}" response="cancel" focusable="true"/>
					</ui:toolbargroup>
				</ui:toolbarbody>
			</ui:dialogtoolbar>
		</ui:dialogpage>
	</body>
</html>