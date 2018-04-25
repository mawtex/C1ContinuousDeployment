<%@ WebHandler Language="C#" Class="ViewFile" %>

using System;
using System.Web;
using Composite.Versioning.ContentVersioning;
using Composite.Data.Types;
using Composite.Core.Extensions;


public class ViewFile : IHttpHandler {
	
	public void ProcessRequest (HttpContext context) {

		Guid activityId = new Guid(context.Request["activityId"]);

		var file = VersioningFacade.GetRestoredItem<IMediaFile>(activityId);

		context.Response.ContentType = file.MimeType;
		
		context.Response.AddHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(file.FileName));
		if (file.Length.HasValue && file.Length > 0)
		{
			//context.Response.AddHeader("Content-Length", file.Length.Value.ToString());
		}
		
		
		var stream = VersioningFacade.GetMediaFileActivityReadStream(activityId);
		stream.CopyTo(context.Response.OutputStream);
		context.Response.Flush();
	}

	public bool IsReusable {
		get {
			return false;
		}
	}

}