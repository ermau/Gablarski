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
		static QueryModule()
		{
			Serializer = new JsonSerializer();
			Serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
		}

		#region Overrides of HttpModule

		/// <summary>
		/// Method that process the url
		/// </summary>
		/// <param name="request">Information sent by the browser about the request</param><param name="response">Information that is being sent back to the client.</param><param name="session">Session used to </param>
		/// <returns>
		/// true if this module handled the request.
		/// </returns>
		public override bool Process (IHttpRequest request, IHttpResponse response, IHttpSession session)
		{
			if (request.UriParts.Length > 0 && request.UriParts[0] == "query")
			{
				ConnectionManager.ProcessSession (session, response);

				var connection = (WebServerConnection)session["connection"];
				var mqueue = (List<MessageBase>)session["mqueue"];
				
				var result = ConnectionManager.SendAndReceive<QueryServerMessage, QueryServerResultMessage> (new QueryServerMessage(), session);

				var bodyWriter = new StreamWriter (response.Body);

				try
				{
					JsonTextWriter writer = new JsonTextWriter (bodyWriter);
					Serializer.Serialize (writer, result);
					writer.Flush();
				}
				catch (Exception ex)
				{
					bodyWriter.Write (ex);
				}

				bodyWriter.Flush();
				response.Send();
				
				return true;
			}

			return false;
		}

		#endregion

		private static readonly JsonSerializer Serializer;
	}
}