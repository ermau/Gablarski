using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;
using Newtonsoft.Json;

namespace Gablarski.WebServer
{
	public class QueryModule
		: HttpModule
	{
		public QueryModule(ConnectionManager manager)
		{
			this.cmanager = manager;
			this.serializer = new JsonSerializer();
			this.serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
		}

		#region Overrides of HttpModule

		public override bool Process (IHttpRequest request, IHttpResponse response, IHttpSession session)
		{
			if (request.UriParts.Length > 0 && request.UriParts[0] == "query")
			{
				cmanager.ProcessSession (session, response);

				var bodyWriter = new StreamWriter (response.Body);
				
				try
				{
					var result = cmanager.SendAndReceive<QueryServerMessage, QueryServerResultMessage> (new QueryServerMessage(), session);
				
					JsonTextWriter writer = new JsonTextWriter (bodyWriter);
					serializer.Serialize (writer, result);

					#if !DEBUG
					response.ContentType = "application/json";
					#endif

					response.AddHeader ("X-JSON", "true");
				}
				catch (Exception ex)
				{
					bodyWriter.Write ("<pre>");
					bodyWriter.Write (ex);
					bodyWriter.Write ("</pre>");
				}

				bodyWriter.Flush();
				response.Send();
				
				return true;
			}

			return false;
		}

		#endregion

		private readonly ConnectionManager cmanager;
		private readonly JsonSerializer serializer;
	}
}