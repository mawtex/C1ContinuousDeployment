using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Composite.Core.ResourceSystem;
using Composite.Data;
using Composite.Data.Types;
using Composite.C1Console.Security;
using Composite.Versioning.ContentVersioning;
using Composite.Versioning.ContentVersioning.ActionTokens;
using Composite.Versioning.ContentVersioning.Data;
using Composite.Versioning.ContentVersioning.Excel;


using SR = Composite.Core.ResourceSystem.StringResourceSystemFacade;

public partial class Composite_InstalledPackages_content_views_ContentVersioning_report : System.Web.UI.Page
{
    private const string LocalizationFile = "Composite.Versioning.ContentVersioning";

    const string ProviderName = "PageElementProvider";
    const string Piggybag = "";
    string _piggybagHash = HashSigner.GetSignedHash(Piggybag).Serialize();


    protected void ContentChanged(Object sender, EventArgs e)
    {

    }

    private string GetShowFunction(EntityToken entityToken, ActionToken actionToken)
    {
        return string.Format("VersioningReport.Show('{0}','{1}','{2}','{3}','{4}');",
            ProviderName,
            EntityTokenSerializer.Serialize(entityToken, true).SerializeToJs(),
            ActionTokenSerializer.Serialize(actionToken, true).SerializeToJs(),
            Piggybag,
            _piggybagHash
        );
    }

    protected void Page_PreRender(object sender, EventArgs e)
    {
        var dataSourceId = DataSourceId.Deserialize(Request.QueryString["DataSourceId"]);
        IData data = DataFacade.GetDataFromDataSourceId(dataSourceId);

        if (data is IMediaFile)
        {
            ViewMediaToolbar.Visible = true;
            ViewMediaContextMenu.Visible = true;
        }
        else
        {
            ViewDataToolbar.Visible = true;
            ViewDataContextMenu.Visible = true;
        }

        using (var dc = new DataConnection())
        {
            var activities = VersioningFacade.GetActivities(dc, dataSourceId);

            activities = activities.Where(item => !(item.FirstOrDefault() == null &&
                                                    (item.Key.TaskType == "Edit" ||
                                                     item.Key.TaskType == "Rollback")));

            string eventTarget = Context.Request.Form["__EVENTTARGET"];
            if (eventTarget == "export")
            {
                GenerateXlsDocument(activities);
            }
            else
            {
                var entityToken = data.GetDataEntityToken();
                BuildReportTable(activities, entityToken);
            }
        }
    }

    void GenerateXlsDocument(IQueryable<IGrouping<ITask, IActivity>> activities)
    {
        var workbook = new Workbook();
        var worksheet = new Worksheet("Sheet1") {GenerateXmlMap = true};
        workbook.Worksheets.Add(worksheet);

        Func<string, Type, Column> column = (label, type) =>
            new Column(SR.GetString(LocalizationFile, label), type);

        var columns = new List<Column>
        {
            column("Table.HeadingTime", typeof (DateTime)),
            column("Table.HeadingUser", typeof (string)),
            column("Table.HeadingTask", typeof (string)),
            column("Table.HeadingActivity", typeof (string))
        };

        worksheet.Columns.AddRange(columns);

        foreach (var item in activities.OrderByDescending(a => a.Key.StartTime).ToList())
        {
            var task = item.Key;

            var subactivities = item.Where(d => d != null).OrderByDescending(d => d.ActivityTime);
            if (!subactivities.Any())
            {
                worksheet.AddRow(task.StartTime, task.Username,
                    SR.GetString(LocalizationFile, "Task." + task.TaskType), string.Empty);
            }

            foreach (var activity in subactivities)
            {
                worksheet.AddRow(activity.ActivityTime, task.Username,
                    SR.GetString(LocalizationFile, "Task." + task.TaskType),
                    SR.GetString(LocalizationFile, "Activity." + activity.ActivityType));
            }
        }

        workbook.WriteToResponse(Context, string.Format("VersioningReport-{0:yyyy-MM-dd-HH-mm}.xls", DateTime.Now));
    }

    void BuildReportTable(IQueryable<IGrouping<ITask, IActivity>> activities, EntityToken entityToken)
    {
        activities = activities.OrderByDescending(a => a.Key.StartTime).Take(int.Parse(Pager.Text));

        XElement report = new XElement("Report",

            activities
                .Select(item => new XElement("Task",
                    item.Key.SerializeProperties(),
                    item.Where(d => d != null).OrderByDescending(d => d.ActivityTime).Select(activity =>
                        new XElement("Activity",
                            activity.SerializeProperties(),
                            new XAttribute("ShowAction", GetShowFunction(
                                entityToken,
                                new DataVersionActionToken(activity.Id, typeof(ShowDataVersionHandleFilter))
                                )
                                ),
                            new XAttribute("ShowFileAction",
                                string.Format("VersioningReport.ShowFile('{0}');", activity.Id.ToString())),
                            new XAttribute("RollbackAction", GetShowFunction(
                                entityToken,
                                new DataVersionActionToken(activity.Id, typeof(RollbackDataVersionHandleFilter))
                                )
                                ),
                            new XAttribute("CompareEntityToken",
                                EntityTokenSerializer.Serialize(new CompareEntityToken(activity.Id.ToString()),
                                    true)),
                            new XAttribute("CompareToActionToken",
                                ActionTokenSerializer.Serialize(new CompareVersionActionToken(activity.Id), true)),
                            new XAttribute("Piggybag", Piggybag),
                            new XAttribute("PiggybagHash", _piggybagHash),
                            null
                            )
                        )
                    )
                )

            );

        var sb = new StringBuilder();
        var xws = new XmlWriterSettings
        {
            OmitXmlDeclaration = true
        };

        using (var writer = XmlWriter.Create(sb, xws))
        {
            var xslTransform = new XslCompiledTransform();
            xslTransform.Load(Server.MapPath(Path.Combine(Page.TemplateSourceDirectory, "report.xslt")));
            xslTransform.Transform(report.CreateReader(), writer);
        }

        Holder.Controls.Add(new LiteralControl(sb.ToString()));
    }
}

public static class Composite_content_views_versioning_report_Extension
{
    private const string LocalizationFile = "Composite.Versioning.ContentVersioning";

    public static IEnumerable<XAttribute> SerializeProperties(this IData data)
	{
		var list = new List<XAttribute>();
		if (data != null)
		{
			foreach (var property in data.DataSourceId.InterfaceType.GetProperties())
			{
			    var name = property.Name;
                var value = property.GetValue(data, null);
			    string label = null;

                if (property.Name == nameof(ITask.TaskType))
                {
                    label = SR.GetString(LocalizationFile, "Task." + value);
                }

                if (property.Name == nameof(IActivity.ActivityType))
                {
                    label = SR.GetString(LocalizationFile, "Activity." + value);
                }

                if (value != null)
				{
					list.Add(new XAttribute(name, value.ToString()));
				}

			    if (label != null)
			    {
			        list.Add(new XAttribute("Label", label));
			    }

            }
		}
		return list;
	}

	public static string SerializeToJs(this string s)
	{
		return s.Replace("\\", "\\\\").Replace("'", "\\'");
	}
}