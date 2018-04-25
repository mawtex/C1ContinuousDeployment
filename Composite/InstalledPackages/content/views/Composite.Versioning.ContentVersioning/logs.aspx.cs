using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Composite.C1Console.Security;
using Composite.Core.PackageSystem.WebServiceClient;
using Composite.Core.ResourceSystem;
using Composite.Core.Types;
using Composite.Data;
using Composite.Data.ProcessControlled;
using Composite.Versioning.ContentVersioning;
using Composite.Versioning.ContentVersioning.Data;
using Composite.Versioning.ContentVersioning.Excel;
using Composite.Versioning.ContentVersioning.ActionTokens;

using SR = Composite.Core.ResourceSystem.StringResourceSystemFacade;
using XlsColumn = Composite.Versioning.ContentVersioning.Excel.Column;

namespace Composite_InstalledPackages_content_views_Composite_Versioning_ContentVersioning
{
	public partial class Logs : System.Web.UI.Page
	{
	    private const string LocalizationFileName = "Composite.Versioning.ContentVersioning";

        string providerName = "PageElementProvider";
		static string piggybag = string.Empty;
		string piggybagHash = HashSigner.GetSignedHash(piggybag).Serialize();


		private Dictionary<Guid, string> _titles = new Dictionary<Guid, string>();
		private Dictionary<Guid, string> _entityTokens = new Dictionary<Guid, string>();
		private Dictionary<Guid, string> _types = new Dictionary<Guid, string>();

		public enum Column
		{
			Time, User, Task
		}

		public bool IsMultiLocale {
			get { return DataLocalizationFacade.ActiveLocalizationCultures.Count() > 1; }
		}

		public Column SortColumn
		{
		    get
		    {
		        int? savedValue = ViewState["SortColumn"] as int?;
                return savedValue != null ? (Column)savedValue : Column.Time;
		    }
			set { ViewState["SortColumn"] = (int)value; }
		}

		public SortDirection SortDirection 
		{
		    get
		    {
		        int? savedValue = ViewState["SortDirection"] as int?;
		        return savedValue != null ? (SortDirection)savedValue : SortDirection.Descending;
		    }
			set { ViewState["SortDirection"] = (int)value; }
		}

		protected void ContentChanged(Object sender, EventArgs e)
		{

		}

		public void CalendarYearClick(object sender, EventArgs e)
		{
			var btn = (LinkButton)sender;
			switch (btn.CommandName)
			{
				case "Back":
					switch (btn.CommandArgument)
					{
						case "From":
							FromDateWidget.VisibleDate = FromDateWidget.VisibleDate.AddYears(-1);
							break;
						case "To":
							ToDateWidget.VisibleDate = ToDateWidget.VisibleDate.AddYears(-1);
							break;
					}
					break;
				case "Forward":
					switch (btn.CommandArgument)
					{
						case "From":
							FromDateWidget.VisibleDate = FromDateWidget.VisibleDate.AddYears(1);
							break;
						case "To":
							ToDateWidget.VisibleDate = ToDateWidget.VisibleDate.AddYears(1);
							break;
					}
					break;
			}
		}

		protected void CalendarSelectionChange(Object sender, EventArgs e)
		{
			if(sender == ToDateWidget) ToDateWidgetPlaceHolder.Visible = false;
			if (sender == FromDateWidget) FromDateWidgetPlaceHolder.Visible = false;
		}

		protected void Refresh(Object sender, EventArgs e)
		{
			using (var connection = new DataConnection())
			{
                if (!connection.Get<ITask>().Any()) return;
				var fromDate = connection.Get<ITask>().Select(d => d.StartTime).Min();
				Username.Text = string.Empty;
				FromDateWidget.SelectedDate = fromDate.Date;
				FromDateWidget.VisibleDate = fromDate.Date;
				FromDateWidgetPlaceHolder.Visible = false;


				ToDateWidget.SelectedDate = DateTime.Now;
				ToDateWidget.VisibleDate = DateTime.Now;
				ToDateWidgetPlaceHolder.Visible = false;
				SetPageNumber(1);
				SortColumn = Column.Time;
				SortDirection = SortDirection.Descending;
			}
		}

		protected void Search(Object sender, EventArgs e)
		{
			SetPageNumber(1);
		}

		protected void Prev(Object sender, EventArgs e)
		{
			var pageNumber = GetPageNumber();
			if (pageNumber > 1)
				SetPageNumber(pageNumber - 1);
		}

		protected void Next(Object sender, EventArgs e)
		{
			var pageNumber = GetPageNumber();
			SetPageNumber(pageNumber + 1);
		}

		protected void SortButton_Click(Object sender, EventArgs e)
		{
			var linkButton = (LinkButton)sender;
			Column column;
			SortDirection direction;
			Enum.TryParse(linkButton.Attributes["SortColumn"], out column);
			Enum.TryParse(linkButton.Attributes["SortDirection"], out direction);
			;

			if (SortColumn == column)
			{
				SortDirection = SortDirection == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
			}
			else
			{
				SortColumn = column;
				SortDirection = direction;
			}
			SetPageNumber(1);
		}

		private void SetPageNumber(int pageNumber)
		{
			PageNumber.Text = pageNumber.ToString();
		}


		protected int GetPageNumber()
		{
			int pageNumber;
			return int.TryParse(PageNumber.Text, out pageNumber) ? pageNumber : 1;
		}

		private int GetPageSize()
		{
			return int.Parse(PageSize.Text);
		}

		public string GetCulture(string cultureName)
		{
			if (string.IsNullOrWhiteSpace(cultureName))
				return string.Empty;
			return new CultureInfo(cultureName).DisplayName;
		}

		public string GetTitle(ITaskTarget taskTarget)
		{
			if (!_titles.ContainsKey(taskTarget.Id))
			{
				_titles[taskTarget.Id] = taskTarget.GetTargetTitle();
			}
			return HttpUtility.HtmlEncode(_titles[taskTarget.Id] ?? string.Empty);
		}

		public string GetTargetType(ITaskTarget taskTarget)
		{
			if (!_types.ContainsKey(taskTarget.Id))
			{
				_types[taskTarget.Id] = taskTarget.GetTargetInterfaceTypeName();
			}
			return _types[taskTarget.Id] ?? string.Empty;

		}

		public string GetEntityToken(ITaskTarget taskTarget)
		{
			if (!_entityTokens.ContainsKey(taskTarget.Id))
			{
				_entityTokens[taskTarget.Id] = taskTarget.GetTargetEntityToken();
			}
			return _entityTokens[taskTarget.Id] ?? string.Empty;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
			    var tasks = new[] {"Add", "Edit", "SendToDraft", "SendForPublication", "Publish", "Unpublish", "Rollback", "Delete"};
			    LogType.DataSource = new[] {new ListItem {Text = SR.GetString(LocalizationFileName, "Filter.Task.All"), Value = "All"}}
			        .Concat(tasks.Select(t => new ListItem {Text = GetTaskLabel(t), Value = t})).ToList();
			    LogType.DataTextField = "Text";
			    LogType.DataValueField = "Value";
                LogType.DataBind();

				DataType.DataSource = new[] { new ListItem { Text = SR.GetString(LocalizationFileName, "Filter.Type.All"), Value = "All" } }
                        .Concat(TargetDataSourceFacade.GetTargetTypes()
						       .Select(d => new ListItem {Text = d.GetShortLabel(), Value = TypeManager.SerializeType(d)})).ToList();
				DataType.DataTextField = "Text";
				DataType.DataValueField = "Value";
				DataType.DataBind();

				Refresh(sender, e);
			}


			string eventTarget = HttpContext.Current.Request.Form["__EVENTTARGET"];
			if (eventTarget == "ToDateSelectorInput")
			{
				ToDateWidgetPlaceHolder.Visible = !ToDateWidgetPlaceHolder.Visible;
				FromDateWidgetPlaceHolder.Visible = false;
			}
			else if (eventTarget == "FromDateSelectorInput")
			{
				FromDateWidgetPlaceHolder.Visible = !FromDateWidgetPlaceHolder.Visible;
				ToDateWidgetPlaceHolder.Visible = false;
			}
		}


		private string GetShowFunction(EntityToken entityToken, ActionToken actionToken)
		{
			return string.Format("VersioningReport.Show('{0}','{1}','{2}','{3}','{4}');",
										providerName,
										EntityTokenSerializer.Serialize(entityToken, true).SerializeToJs(),
										ActionTokenSerializer.Serialize(actionToken, true).SerializeToJs(),
										piggybag,
										piggybagHash
			);
		}

		private string GetRestoreAction(IActivity activity)
		{
			if (activity != null && activity.ActivityType == "Delete")
			{
				return GetShowFunction(activity.GetDataEntityToken(), new RestoreActionToken());
			}
			return string.Empty;
		}

		protected void Page_PreRender(object sender, EventArgs e)
		{
			var pageNumber = GetPageNumber();
			var pageSize = GetPageSize();

			using (var connection = new DataConnection())
			{
				Expression<Func<ITask, bool>> taskFilter = t => true;
				Expression<Func<ITaskTarget, bool>> targetFilter = t => true;

				if (!string.IsNullOrWhiteSpace(Username.Text))
				{
					taskFilter = taskFilter.And(d => d.Username.Contains(Username.Text));
				}
				if (LogType.SelectedValue != "All" && !string.IsNullOrWhiteSpace(LogType.SelectedValue))
				{
					taskFilter = taskFilter.And(d => d.TaskType == LogType.SelectedValue);
				}
				if (DataType.SelectedValue != "All" && !string.IsNullOrWhiteSpace(DataType.SelectedValue))
				{
					targetFilter = targetFilter.And(d => d.TargetDataSourceId.Contains(string.Format(@"_interfaceType_='{0}'", DataType.SelectedValue.Replace(".",@"\."))));
				}

				var activities = from t in connection.Get<ITask>().Where(taskFilter)
								 join tt in connection.Get<ITaskTarget>().Where(targetFilter) on t.TaskTargetId equals tt.Id
								 join a in connection.Get<IActivity>() on t.Id equals a.TaskId into ats
								 from at in ats.DefaultIfEmpty()
								 select new
									 {
										 Time = at == null ? t.StartTime : at.ActivityTime,
										 t.Username,
										 t.TaskType,
										 ActivityType = at == null ? null : at.ActivityType,
										 t.CultureName,
										 TaskTarget = tt,
										 Activity = at
									 };

				var from = FromDateWidget.SelectedDate;
				var to = ToDateWidget.SelectedDate;
				activities = activities.Where(d => d.Time >= from && d.Time < to.AddDays(1));


				activities = activities.Where(item => !(item.ActivityType == null &&
														new[] { "edit", "add", "rollback", "delete" }.Contains(item.TaskType.ToLower())))
									   .OrderByDescending(d => d.Time);

				switch (SortColumn)
				{
					case Column.Time:
						activities = SortDirection == SortDirection.Ascending
							? activities.OrderBy(d => d.Time)
							: activities.OrderByDescending(d => d.Time);
						break;
					case Column.User:
						activities = SortDirection == SortDirection.Ascending
							? activities.OrderBy(d => d.Username)
							: activities.OrderByDescending(d => d.Username);
						break;
					case Column.Task:
						activities = SortDirection == SortDirection.Ascending
							? activities.OrderBy(d => d.TaskType)
							: activities.OrderByDescending(d => d.TaskType);
						break;

				}



				string eventTarget = Request.Form["__EVENTTARGET"];
				if (eventTarget == "export")
				{

					var workbook = new Workbook();
					var worksheet = new Worksheet("Sheet1") { GenerateXmlMap = true };
					workbook.Worksheets.Add(worksheet);
                    Func<string, Type, XlsColumn> column = (label, type) =>
                        new XlsColumn(SR.GetString(LocalizationFileName, label), type);

				    var columns = new List<XlsColumn>
				    {
				        column("Table.HeadingTime", typeof (DateTime)),
				        column("Table.HeadingUser", typeof (string)),
				        column("Table.HeadingTask", typeof (string)),
				        column("Table.HeadingActivity", typeof (string)),
				        column("Table.HeadingTitle", typeof (string)),
				        column("Table.HeadingType", typeof (string))
				    };


					if (IsMultiLocale)
					{
						columns.Add(new XlsColumn(SR.GetString("Composite.Plugins.XsltBasedFunction", "EditXsltFunction.LabelActiveLocales"), typeof(string)));
					}

					worksheet.Columns.AddRange(columns);

                    foreach (var item in activities)
                    {
                        var parameters = new object[]
                        {
                            item.Time,
                            item.Username ?? string.Empty,
                            item.TaskType != null ? GetTaskLabel(item.TaskType) : string.Empty,
                            item.ActivityType != null ? GetActivityLabel(item.ActivityType) : string.Empty,
                            GetTitle(item.TaskTarget),
                            GetTargetType(item.TaskTarget)
                        };

                        if (IsMultiLocale)
                        {
                            parameters = parameters.Concat(new object[] {GetCulture(item.CultureName)}).ToArray();
                        }

                        worksheet.AddRow(parameters);
                    }

                    workbook.WriteToResponse(Context, string.Format("ContentModificationLog-{0:yyyy-MM-dd-HH-mm}.xls", DateTime.Now));
					return;
				}
				
				activities = activities.Skip(pageSize * (pageNumber - 1)).Take(pageSize + 1);

				PrevPage.Attributes["client_isdisabled"] = (pageNumber == 1).ToString().ToLower();
				NextPage.Attributes["client_isdisabled"] = (activities.Count() <= pageSize).ToString().ToLower();


				var result = activities.Take(pageSize).ToList();
				LogHolder.DataSource = result.Select(d=>
					new
					{
						d.Time,
						d.Username,
						d.ActivityType,
                        ActivityLabel = string.IsNullOrEmpty(d.ActivityType) ? "" : GetActivityLabel(d.ActivityType),
						d.TaskType,
                        TaskLabel = GetTaskLabel(d.TaskType),
						Title = GetTitle(d.TaskTarget),
						EntityToken = GetEntityToken(d.TaskTarget),
						TargetType = GetTargetType(d.TaskTarget),
						d.CultureName,
						RestoreAction = GetRestoreAction(d.Activity)
					}).ToList();
				LogHolder.DataBind();
            }

			SetPageNumber(pageNumber);

		}

	    private static string GetTaskLabel(string task)
	    {
	        return SR.GetString(LocalizationFileName, "Task." + task);
	    }

	    private static string GetActivityLabel(string activity)
	    {
	        return SR.GetString(LocalizationFileName, "Activity." + activity);
	    }

        
    }

	internal static class ExpressionExtensionMethods
	{
		public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expression1,
													  Expression<Func<T, bool>> expression2)
		{
			var invokedExpression = Expression.Invoke(expression2, expression1.Parameters);
			return Expression.Lambda<Func<T, bool>>
				(Expression.Or(expression1.Body, invokedExpression), expression1.Parameters);
		}



		public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expression1,
													   Expression<Func<T, bool>> expression2)
		{
			var invokedExpression = Expression.Invoke(expression2, expression1.Parameters);
			return Expression.Lambda<Func<T, bool>>
				(Expression.And(expression1.Body, invokedExpression), expression1.Parameters);
		}

		public static string SerializeToJs(this string s)
		{
			return s.Replace("\\", "\\\\").Replace("'", "\\'");
		}
	}
}