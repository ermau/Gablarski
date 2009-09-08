using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;

namespace Gablarski.WebServer
{
	public class AdminModule
		: HttpModule
	{
		#region Overrides of HttpModule

		public override bool Process (IHttpRequest request, IHttpResponse response, IHttpSession session)
		{
			if (request.UriParts.Length > 0 && request.UriParts[0] == "admin")
			{
				ConnectionManager.ProcessSession (session, response);
				try
				{
					if (!((bool)session["loggedIn"]))
						response.Redirect (new Uri ("http://" + request.Uri.Host + "/admin/login.html", UriKind.Absolute));
					else
					{
						if (request.UriParts.Length > 1)
						{
							switch (request.UriParts[1])
							{
								
							}
						}
						else
						{
							
						}
					}
				}
				catch (Exception ex)
				{
					var writer = new StreamWriter (response.Body);
					writer.Write ("<pre>");
					writer.Write (ex);
					writer.Write ("</pre>");
					writer.Flush();
					response.Send();
				}

				return true;
			}

			return false;
		}

		#endregion

		private void ShowLogin()
		{
			
		}
	}
}