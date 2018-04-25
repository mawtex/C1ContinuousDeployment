using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Composite.Data;
using Composite.Data.DynamicTypes;
using Composite.Data.Types;
using Composite.Core.Types;
using Composite.Versioning.ContentVersioning;
using Composite.Versioning.ContentVersioning.Data;
using Composite.Core.Xml;
using TidyNet;
using Composite.Versioning.ContentVersioning.Excel;

using SR = Composite.Core.ResourceSystem.StringResourceSystemFacade;

public partial class Composite_InstalledPackages_content_views_ContentVersioning_compare : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		var compareId = new Guid(Request["compare"]);
		var compareToId = new Guid(Request["compareTo"]);

		var compareData = DataFacade.GetData<IDataChanges>(d => d.ActivityId == compareId).ToList();
		var compareToData = DataFacade.GetData<IDataChanges>(d => d.ActivityId == compareToId).ToList();


		var compareReport = new XElement("CompareReport");

		var targetIds = compareData.Select(d => d.TargetId).Union(compareToData.Select(d => d.TargetId));

        Func<IEnumerable<IDataChanges>, Guid, XElement> getXml = (data, targetId) =>
            XElement.Parse(data.Where(d => d.TargetId == targetId).Select(d => d.SerializedData).FirstOrDefault()
                           ?? "<Data />");

        foreach (var targetId in targetIds)
		{
            var compareXml = getXml(compareData, targetId);
            var compareToXml = getXml(compareToData, targetId);

		    Guid? pageId = ExtractPageId(compareXml) ?? ExtractPageId(compareToXml);

            var helper = new DataTypeHelper(targetId, pageId);

			var Data = new XElement("Data", new XAttribute("caption", helper.GetCaption()));


			
			var attributeNames = compareXml.Attributes().Select(a => a.Name.LocalName)
                          .Union(compareToXml.Attributes().Select(a => a.Name.LocalName))
                          .Except(helper.GetKeyPropertyNames());

			foreach (var attributeName in attributeNames)
			{
				var compareField = compareXml.Attribute(attributeName);
				var compareToField = compareToXml.Attribute(attributeName);

				var property = new XElement("Property",
					new XAttribute("Label", helper.GetFieldLabel(attributeName)),
					new XAttribute("Equals", helper.EqualsValueAttributes(compareField, compareToField)),
					new XAttribute("IsMarkup", ismarkupystify(helper.GetValue(compareField))),
					new XElement("Compare", stringelingify ( helper.GetValue(compareField))),
					new XElement("CompareTo", stringelingify ( helper.GetValue(compareToField)))
				);

				Data.Add(property);
			}

			compareReport.Add(Data);
		}

		var compareTable = new XElement("compareTable");

        var sb = new StringBuilder();
        var xws = new XmlWriterSettings()
        {
            OmitXmlDeclaration = true
        };

        using (var writer = XmlWriter.Create(sb, xws))
		{
		
			var settings = new XsltSettings(false, true);
		
			var xslTransform = new XslCompiledTransform();
			xslTransform.Load(
				Server.MapPath(Path.Combine(Page.TemplateSourceDirectory, "compare.xslt")),
				settings,
				new XmlUrlResolver()	
			);
			xslTransform.Transform(compareReport.CreateReader(), writer);

		}

		Holder.Controls.Add(new LiteralControl(sb.ToString()));
		
		string eventTarget = Request.Form["__EVENTTARGET"];
		if (eventTarget == "export")
		{

			var workbook = new Workbook();
			var worksheet = new Worksheet("Sheet1") { GenerateXmlMap = true };
			workbook.Worksheets.Add(worksheet);
			var columns = new List<Column>()
			{
                new Column(string.Empty, typeof(string)),
                new Column(SR.GetString("Composite.Versioning.ContentVersioning", "Table.HeadingNew"), typeof(string)),
                new Column(SR.GetString("Composite.Versioning.ContentVersioning", "Table.HeadingOld"), typeof(string))
            };

			worksheet.Columns.AddRange(columns);

			foreach (var data in compareReport.Elements("Data"))
			{
				worksheet.AddRow(data.Attribute("caption").Value, string.Empty, string.Empty);

				foreach (var property in data.Elements("Property"))
				{
					var equals = property.Attribute("Equals").Value == "true";
					var label = property.Attribute("Label").Value;
					var newvalue = property.Element("Compare").Value;
					var oldvalue = property.Element("CompareTo").Value;
					DateTime date;
					if (DateTime.TryParse(newvalue, out date)) newvalue = date.ToString("yyyy-MM-dd HH:mm:ss");
					if (DateTime.TryParse(oldvalue, out date)) oldvalue = date.ToString("yyyy-MM-dd HH:mm:ss");

					worksheet.AddBoldRow(!equals, label, newvalue, oldvalue);
				}

				worksheet.AddRow(string.Empty, string.Empty, string.Empty);
			}

			workbook.WriteToResponse(Context, string.Format("VersioningReport-{0:yyyy-MM-dd-HH-mm}.xls", DateTime.Now));
			return;
		}
	}

	internal IEnumerable<XNode> stringelingify (IEnumerable<XNode> nodes)
	{
		foreach (XNode node in nodes)
		{
			yield return new XText(node.ToString());
		}
	}

	internal bool ismarkupystify(IEnumerable<XNode> nodes)
	{
		return nodes.Any(f => f is XElement);
	}

    Guid? ExtractPageId(XElement element)
    {
        if (element != null)
        {
            var pageIdStr = (string) element.Attribute("PageId");
            Guid result;

            if (pageIdStr != null && Guid.TryParse(pageIdStr, out result))
            {
                return result;
            }
        }

        return null;
    }

	internal class DataTypeHelper
	{
		private DataTypeDescriptor _dataTypeDescriptor;
		private DataSourceId _dataSourceId;
		public DataTypeHelper(string targetDataSourceId, Guid? pageId)
		{
			
			DataSourceId dataSourceId;
			DataSourceIdHelper.TryDeserialize(targetDataSourceId, pageId, out dataSourceId);

			if (dataSourceId != null)
			{
				_dataSourceId = dataSourceId;
				_dataTypeDescriptor = DynamicTypeManager.GetDataTypeDescriptor(dataSourceId.InterfaceType);
			}
				
		}

		public DataTypeHelper(Guid targetId, Guid? pageId)
		{

			DataSourceId dataSourceId;
			TargetDataSourceFacade.TryDeserialize(targetId, pageId, out dataSourceId);

			if (dataSourceId != null)
			{
				_dataSourceId = dataSourceId;
				_dataTypeDescriptor = DynamicTypeManager.GetDataTypeDescriptor(dataSourceId.InterfaceType);
			}

		}

		internal string GetFieldLabel(string name)
		{
			if (_dataTypeDescriptor != null)
			{
				var field = _dataTypeDescriptor.Fields[name];
				if (field != null && field.FormRenderingProfile != null && field.FormRenderingProfile.Label != null)
				{
					return field.FormRenderingProfile.Label;
				}
			}	
			return name;
		}

		internal IEnumerable<XNode> GetValue(XAttribute attribute)
		{
			if(attribute == null)
				yield break;

			var value = attribute.Value;

			if (_dataSourceId != null)
			{
				bool valueContainHtml = value != null && value.IndexOf("<p>") > -1;
				if (valueContainHtml 
                    || _dataSourceId.InterfaceType == typeof(IPagePlaceholderContent) 
                    && attribute.Name.LocalName == "Content")
				{
					string xhtml = value;
					try
					{
						// This ensures that html may be a complete xhtml document, not just a fragment.
					    string xhtmlDocString = value.Contains("<html")
					        ? value
					        : string.Format("<html xmlns='{0}'><head></head><body>{1}</body></html>", Namespaces.Xhtml, value);

						XElement doc = XElement.Parse(xhtmlDocString);

						xhtml = doc.ToString(SaveOptions.DisableFormatting);

						//byte[] htmlByteArray = Encoding.UTF8.GetBytes(xhtml);
						//using (MemoryStream inputStream = new MemoryStream(htmlByteArray))
						//{
						//	using (MemoryStream outputStream = new MemoryStream())
						//	{
						//		Tidy tidy = GetXhtmlConfiguredTidy();
						//		TidyMessageCollection tidyMessages = new TidyMessageCollection();

						//		try
						//		{
						//			tidy.Parse(inputStream, outputStream, tidyMessages);
						//			if (tidyMessages.Errors == 0)
						//			{
						//				outputStream.Position = 0;
						//				StreamReader sr = new StreamReader(outputStream);
						//				xhtml = sr.ReadToEnd();
						//			}
						//		}
						//		catch (Exception) { } // Tidy clean up failures supressed
						//	}
						//}

					}
					catch { }
					yield return XElement.Parse(xhtml);
					yield break;
				}
				PropertyInfo propertyInfo = _dataSourceId.InterfaceType.GetProperty(attribute.Name.LocalName);
				if (propertyInfo != null)
				{
					
					ForeignKeyAttribute foreignKeyAttribute = propertyInfo.GetCustomAttributesRecursively<ForeignKeyAttribute>().FirstOrDefault();

					object propertyValue;
					IData foreignData = null;

					try{
						propertyValue = ValueTypeConverter.Convert(value, propertyInfo.PropertyType);
						

						if (foreignKeyAttribute != null)
						{
							var keyPropertyNames  = foreignKeyAttribute.InterfaceType.GetKeyPropertyNames();
							if(keyPropertyNames.Count == 1)
							{
								var foreignKeyProperty = foreignKeyAttribute.InterfaceType.GetProperty(keyPropertyNames.First());
								foreignData = DataFacade.GetData(foreignKeyAttribute.InterfaceType).ToDataEnumerable().Where(d => propertyValue.Equals(foreignKeyProperty.GetValue(d, null))).FirstOrDefault();
							}

						
					}
					}catch{}
					if (foreignData != null)
					{
						yield return new XText(foreignData.GetLabel());
						yield break;
					}

				}
			}
			
			yield return new XText(value);
		}

		internal bool EqualsValueAttributes(XAttribute attr1, XAttribute attr2)
		{
			if (attr1 == null && attr2 == null) return true;

            return (attr1 != null && attr2 != null) && attr1.Value == attr2.Value;
		}

		internal string GetCaption()
		{

			if (_dataSourceId != null)
			{
				try
				{
					TypeManager.SerializeType(_dataSourceId.InterfaceType);
				}
				catch{ }

				return _dataSourceId.InterfaceType.GetShortLabel();
			}
			return "";
		}

        internal IEnumerable<string> GetKeyPropertyNames()
        {
            return _dataSourceId != null
                ? _dataSourceId.InterfaceType.GetKeyPropertyNames()
                : Enumerable.Empty<string>();
        }

        private static Tidy GetXhtmlConfiguredTidy()
		{
			return new Tidy
            {
                Options =
                {
                    RawOut = true,
                    TidyMark = false,

                    CharEncoding = CharEncoding.UTF8,
                    DocType = DocType.Omit,
                    AllowElementPruning = false,
                    WrapLen = 0,
                    TabSize = 2,
                    Spaces = 4,
                    SmartIndent = true,

                    BreakBeforeBR = false,
                    DropEmptyParas = false,
                    Word2000 = false,
                    MakeClean = false,
                    Xhtml = true,
                    XmlOut = false,
                    XmlTags = false,

                    QuoteNbsp = false,
                    NumEntities = true,
                }
            };
		}
	}
}
