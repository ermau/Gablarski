using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Client;

namespace Gablarski.Clients.CLI
{
	public class ClientModule
		: GClientModule
	{
		public ClientModule (GablarskiClient client, TextWriter writer)
			: base (client, writer)
		{
			client.Connected += OnConnected;
		}

		void OnConnected (object sender, EventArgs e)
		{
			Writer.WriteLine ("Connected");
		}

		public override bool Process (string line)
		{
			var parts = CommandLine.Parse (line);
			if (parts[0] != "client")
				return false;

			if (parts.Count == 1)
			{
				Writer.WriteLine ("client commands:");
				Writer.WriteLine ("connect - connects to a server");
				Writer.WriteLine ("join - joins the server as a user");
				Writer.WriteLine ("disconnect - disconnects from the current server");
				return true;
			}

			switch (parts[1])
			{
				case "connect":
				{
					if (parts.Count < 3 || parts.Count == 4 || parts.Count > 5)
					{
						Writer.WriteLine ("client connect <host[:port]>");
						Writer.WriteLine ("client connect <host[:port]> <username> <password>");
						return true;
					}

					string[] hostParts = parts[2].Split (':');
					int port = (hostParts.Length == 1) ? 6112 : Int32.Parse (hostParts[1]);

					Client.Connect (hostParts[0], port);

					return true;
				}
				
				case "disconnect":
				{
					if (Client.IsConnected)
						Client.Disconnect();
					else
						Writer.WriteLine ("Not connected");
					
					return true;
				}

				case "join":
				{
					if (parts.Count < 3 || parts.Count > 5)
					{
						Writer.WriteLine ("client join <nickname>");
						Writer.WriteLine ("client join <nickname> <phonetic>");
						Writer.WriteLine ("client join <nickname> <phonetic> <serverpassword>");
						return true;
					}

					if (parts.Count == 3)
						Client.CurrentUser.Join (parts[2], null);
					else if (parts.Count == 4)
						Client.CurrentUser.Join (parts[2], parts[3], null);
					else
						Client.CurrentUser.Join (parts[2], parts[3], parts[4]);

					return true;
				}

				default:
					return false;
			}
		}
	}
}