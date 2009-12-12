using Gablarski.Server;
using HttpServer.Controllers;
using HttpServer.Rendering;

namespace Gablarski.WebServer
{
	public class LoginController
		: ViewController
	{
		public LoginController (TemplateManager templates, ConnectionManager connectionManager, IServerContext clientContext)
			: base(templates)
		{
			this.connections = connectionManager;
			this.server = clientContext;
		}

		public string Index()
		{
			return Render ("serverName", server.Settings.Name);
		}

		public override object Clone()
		{
			return this;
		}

		private readonly ConnectionManager connections;
		private readonly IServerContext server;
	}
}