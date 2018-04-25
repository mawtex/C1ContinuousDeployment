<%@ Page Language="C#" AutoEventWireup="true" CodeFile="logs.aspx.cs" Inherits="Composite_InstalledPackages_content_views_Composite_Versioning_ContentVersioning.Logs" %>
<%@ Import Namespace="Composite.Versioning.ContentVersioning" %>
<%@ Import Namespace="Composite.Versioning.ContentVersioning.Data" %>

<html xmlns="http://www.w3.org/1999/xhtml" xmlns:ui="http://www.w3.org/1999/xhtml"
xmlns:control="http://www.composite.net/ns/uicontrol">
<control:httpheaders runat="server" />
<head>
	<title>Composite.Management.VersioningReport</title>
	<control:styleloader runat="server" />
	<control:scriptloader type="sub" runat="server" />
	<link rel="stylesheet" type="text/css" href="logs.css.aspx" />
	<link rel="stylesheet" type="text/css" href="print.css.aspx" media="print" />
	<script type="text/javascript" src="report.js" />
	<script type="text/javascript" src="bindings/ReportLabelBinding.js"></script>
	<script type="text/javascript" src="bindings/ReportTableBinding.js"></script>
	<script type="text/javascript" src="bindings/ReportToolbarBinding.js"></script>
	<script type="text/javascript" src="bindings/LogToolbarBinding.js"></script>
	<script type="text/javascript" src="bindings/LogTitleBinding.js"></script>
</head>
<body id="root">
	<form id="Form1" runat="server" class="updateform updatezone">
		<ui:broadcasterset>
			<ui:broadcaster id="broadcasterHasSelection" isdisabled="true" />
		</ui:broadcasterset>
		<ui:popupset>
		</ui:popupset>
		<ui:cursor id="comparecursor" image="${icon:versioning-compare}" />
		<ui:page id="page" image="${icon:report}">
			<ui:toolbar id="toolbar" binding="LogToolBarBinding">
				<ui:toolbarbody>
					<ui:toolbargroup>
						<aspui:toolbarbutton id="RefreshButton" autopostback="true" text="${string:Composite.Versioning.ContentVersioning:ToolbarButton.LabelRefresh}" imageurl="${icon:refresh}"
							runat="server" onclick="Refresh" />
					</ui:toolbargroup>
					<ui:toolbargroup>
						<ui:toolbarbutton id="exportbutton" label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.Export}"  cmd="export" image="${icon:mimetype-xls}" />
					</ui:toolbargroup>
					<ui:toolbargroup>
						<ui:toolbarbutton id="printbutton" label="${string:FunctionDocumentation.LabelButtonPrint}"
							oncommand="print()" image="${icon:print}"
							tooltip="${string:FunctionDocumentation.LabelButtonPrint}" />
					</ui:toolbargroup>
					<ui:toolbargroup>
					<ui:toolbarbutton label="${string:Composite.Versioning.ContentVersioning:ToolbarButton.LabelRestore}"
						cmd="restore" image="${icon:versioning-restore}" image-disabled="${icon:versioning-restore-disabled}"
						tooltip="${string:Composite.Versioning.ContentVersioning:ToolbarButton.ToolTipRestore}"
						observes="broadcasterHasSelection" />
				</ui:toolbargroup>
				</ui:toolbarbody>
			</ui:toolbar>
			<ui:flexbox id="flexbox">
				<ui:scrollbox id="scrollbox">
					<table id="filter">
						<thead>
							<tr>
								<th><ui:text label="${string:Composite.Versioning.ContentVersioning:Filter.From}" /> </th>
								<th class="input">
									<div class="datecontainer">
										<div>
											<ui:datainputbutton callbackid="FromDateSelectorInput"
												image="${icon:fields}" value="<%= FromDateWidget.SelectedDate.ToShortDateString() %>"
												readonly="true" />
										</div>
										<div class="widget">
											<asp:PlaceHolder ID="FromDateWidgetPlaceHolder" Visible="False"
												runat="server">
												<div class="calendar">
													<asp:Calendar ID="FromDateWidget" runat="server" ShowDayHeader="true" OnSelectionChanged="CalendarSelectionChange"
														OtherMonthDayStyle-CssClass="othermonth" SelectedDayStyle-CssClass="selectedday" />
													<asp:LinkButton ID="LinkButton3" CssClass="calendaryearback" CommandName="Back" CommandArgument="From" OnClick="CalendarYearClick"
														runat="server">back</asp:LinkButton>
													<asp:LinkButton ID="LinkButton4" CssClass="calendaryearforward" CommandName="Forward" CommandArgument="From" OnClick="CalendarYearClick"
														runat="server">forward</asp:LinkButton>
												</div>
											</asp:PlaceHolder>
										</div>
									</div>
								</th>
								<th><ui:text label="${string:Composite.Versioning.ContentVersioning:Filter.To}" /> </th>
								<th class="input">
									<div class="datecontainer">
										<div>
											<ui:datainputbutton callbackid="ToDateSelectorInput"
												image="${icon:fields}" value="<%= ToDateWidget.SelectedDate.ToShortDateString() %>"
												readonly="true" />
										</div>
										<div class="widget">
											<asp:PlaceHolder ID="ToDateWidgetPlaceHolder" Visible="False"
												runat="server">
												<div class="calendar">
													<asp:Calendar ID="ToDateWidget" runat="server" ShowDayHeader="true" OnSelectionChanged="CalendarSelectionChange"
														OtherMonthDayStyle-CssClass="othermonth" SelectedDayStyle-CssClass="selectedday" />
													<asp:LinkButton ID="LinkButton1" CssClass="calendaryearback"  CommandName="Back" CommandArgument="To" OnClick="CalendarYearClick"
														runat="server">back</asp:LinkButton>
													<asp:LinkButton ID="LinkButton2" CssClass="calendaryearforward" CommandName="Forward" CommandArgument="To" OnClick="CalendarYearClick"
														runat="server">forward</asp:LinkButton>
												</div>
											</asp:PlaceHolder>
										</div>
									</div>
								</th>
								<th><ui:text label="${string:Composite.Versioning.ContentVersioning:Filter.User}" /></th>
								<th>
									<aspui:datainput runat="server" id="Username" client_callbackid="UsernameInput" client_autopost="true" />

								</th>
								<th><ui:text label="${string:Composite.Versioning.ContentVersioning:Table.HeadingTask}" /> </th>
								<th>
									<aspui:selector runat="server" id="LogType" autopostback="True" />
								</th>
								<th><ui:text label="${string:Composite.Versioning.ContentVersioning:Table.HeadingType}" /> </th>
								<th>
									<aspui:selector runat="server" id="DataType" autopostback="True" />
								</th>
								<th width="100%">

								</th>

							</tr>
						</thead>
					</table>
					<table width="100%" id="logtable" binding="ReportTableBinding">
						<thead>
							<tr>
								<th class="time button updatezone">
									<aspui:toolbarbutton runat="server" ID="SortTimeButton" Text="${string:Composite.Versioning.ContentVersioning:Table.HeadingTime}" OnClick="SortButton_Click" SortColumn="Time" SortDirection="Descending" />
								</th>
								<th class="user button">
									<aspui:toolbarbutton runat="server" ID="SortUserButton" Text="${string:Composite.Versioning.ContentVersioning:Table.HeadingUser}" OnClick="SortButton_Click" SortColumn="User" SortDirection="Ascending" />
								</th>
								<th class="task button">
									<aspui:toolbarbutton runat="server" ID="SortTaskButton" Text="${string:Composite.Versioning.ContentVersioning:Table.HeadingTask}" OnClick="SortButton_Click" SortColumn="Task" SortDirection="Ascending" />
								</th>
								<th class="activity">
									<ui:text label="${string:Composite.Versioning.ContentVersioning:Table.HeadingActivity}" />
								</th>
								<th class="title">
									<ui:text label="${string:Composite.Versioning.ContentVersioning:Table.HeadingTitle}" />
								</th>
								<th class="type">
									<ui:text label="${string:Composite.Versioning.ContentVersioning:Table.HeadingType}" />
								</th>
								<% if (IsMultiLocale)
								   { %>
								<th>
									<ui:text label="${string:Composite.Plugins.XsltBasedFunction:EditXsltFunction.LabelActiveLocales}" />
								</th>
								<% } %>
							</tr>
						</thead>
						<tbody>
							<asp:Repeater runat="server" ID="LogHolder" ViewStateMode="Disabled">
								<ItemTemplate>
									<tr restoreaction="<%#Server.HtmlEncode((string)Eval("RestoreAction"))%>">
										<td>
											<%#Eval("Time") %>
										</td>
										<td>
											<ui:labelbox label="<%#Eval("Username") %>" image="${icon:user}" />
										</td>
										<td>
											<ui:labelbox label="<%# Eval("TaskLabel")%>" imagekey="<%#Eval("TaskType") %>" versioningrole="task" binding="ReportLabelBinding" />
										</td>
										<td>
											<%#Eval("ActivityLabel")%>
										</td>
										<td class="title">
											<ui:labelbox label="<%#Eval("Title")%>" binding="LogTitleBinding"
												entitytoken="<%#Server.HtmlEncode((string)Eval("EntityToken"))%>" />
										</td>
										<td>
											<%#Eval("TargetType")%>
										</td>
											<% if (IsMultiLocale)
											   { %>
										<td>
											<%#GetCulture((string)Eval("CultureName"))%>
										</td>
										<% } %>
									</tr>
								</ItemTemplate>
							</asp:Repeater>
						</tbody>
					</table>
				</ui:scrollbox>
				<ui:toolbar id="bottomtoolbar">
					<ui:toolbarbody>
						<ui:toolbargroup>
							<aspui:selector runat="server" id="PageSize" autopostback="true" onselectedindexchanged="Search">
						<asp:ListItem>25</asp:ListItem>
						<asp:ListItem>100</asp:ListItem>
						<asp:ListItem>500</asp:ListItem>
						<asp:ListItem Text="${string:Composite.Versioning.ContentVersioning:Toolbar.RowsToShow.LabelAll}" Value="1000" />
					</aspui:selector>
						</ui:toolbargroup>
						<ui:toolbargroup>
							<aspui:toolbarbutton runat="server" ID="PrevPage" client_tooltip="Previous" client_image="${icon:previous}" client_image-disabled="${icon:previous-disabled}"
								onclick="Prev"></aspui:toolbarbutton>
							<aspui:textbox runat="server" readonly="True" id="PageNumber" width="20" />
							
							<aspui:toolbarbutton runat="server" ID="NextPage" client_tooltip="Next" client_image="${icon:next}" client_image-disabled="${icon:next-disabled}"
								onclick="Next"></aspui:toolbarbutton>
								
						</ui:toolbargroup>
					</ui:toolbarbody>
				</ui:toolbar>
				<ui:cover id="cover" hidden="true" />
			</ui:flexbox>
		</ui:page>
	</form>
</body>
</html>
