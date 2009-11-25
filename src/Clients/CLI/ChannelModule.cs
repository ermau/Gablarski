using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Client;
using Mono.Options;

namespace Gablarski.Clients.CLI
{
	public class ChannelModule
		: GClientModule
	{
		public ChannelModule (GablarskiClient client)
			: base (client)
		{
		}

		public override bool Process (string command, TextWriter writer)
		{
			string[] parts = command.Split (' ');
			if (parts.Length == 0 || parts[0] != "channel")
				return false;

			if (parts.Length == 1)
			{
				writer.WriteLine ("channel list - Lists channels and occupants");
				writer.WriteLine ("channel join <channelID|Name> - Joins the specified channel");
				writer.WriteLine ("channel move <userID|Name> <channelID|Name> - Moves the specified user to the specified channel");
				return true;
			}

			switch (parts[1])
			{
				case "list":
				{
					writer.WriteLine ("Channels:");
					foreach (var c in Client.Channels)
					{
						writer.WriteLine ("{0}: {1}", c.ChannelId, c.Name);
						writer.WriteLine (c.Description);

						if (c.UserLimit > 0)
						{
							var users = Client.Users.Where (cu => cu.CurrentChannelId == c.ChannelId).ToList();
							writer.WriteLine ("Users: {0} / {1}", users.Count, c.UserLimit);
							foreach (var u in users)
								writer.WriteLine (u);

							writer.WriteLine();
						}
						else
							writer.WriteLine (Client.Users.Where (cu => cu.CurrentChannelId == c.ChannelId).Count());

						writer.WriteLine();
					}

					return true;
				}

				case "join":
				{
					if (parts.Length != 3)
					{
						writer.WriteLine ("channel join <id|name>");
						return true;
					}

					ChannelInfo channel = FindChannel (parts[2]);

					if (channel != null)
						Client.Users.Move (Client.CurrentUser, channel);
					else
						writer.WriteLine ("Channel {0} not found.", parts[2]);

					return true;
				}

				case "move":
				{
					if (parts.Length != 4)
					{
						writer.WriteLine ("channel move <userID|Name> <channelID|Name>");
						return true;
					}

					UserInfo user = FindUser (parts[2]);
					if (user == null)
					{
						writer.WriteLine ("User {0} not found.", parts[2]);
						return true;
					}

					ChannelInfo channel = FindChannel (parts[3]);
					if (channel == null)
					{
						writer.WriteLine ("Channel {0} not found.", parts[3]);
						return true;
					}

					Client.Users.Move (user, channel);

					return true;
				}

				default:
					return false;
			}
		}
	}
}