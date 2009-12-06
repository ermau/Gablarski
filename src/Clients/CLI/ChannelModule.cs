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
		public ChannelModule (GablarskiClient client, TextWriter writer)
			: base (client, writer)
		{
		}

		public override bool Process (string command)
		{
			string[] parts = command.Split (' ');
			if (parts[0] != "channel")
				return false;

			if (parts.Length == 1)
			{
				Writer.WriteLine ("channel list - Lists channels and occupants");
				Writer.WriteLine ("channel join <channelID|Name> - Joins the specified channel");
				Writer.WriteLine ("channel move <userID|Name> <channelID|Name> - Moves the specified user to the specified channel");
				return true;
			}

			switch (parts[1])
			{
				case "list":
				{
					Writer.WriteLine ("Channels:");
					foreach (var c in Client.Channels)
					{
						Writer.WriteLine ("{0}: {1}", c.ChannelId, c.Name);
						Writer.WriteLine (c.Description);

						if (c.UserLimit > 0)
						{
							var users = Client.Users.Where (cu => cu.CurrentChannelId == c.ChannelId).ToList();
							Writer.WriteLine ("Users: {0} / {1}", users.Count, c.UserLimit);
							foreach (var u in users)
								Writer.WriteLine (u);

							Writer.WriteLine ();
						}
						else
							Writer.WriteLine (Client.Users.Where (cu => cu.CurrentChannelId == c.ChannelId).Count ());

						Writer.WriteLine ();
					}

					return true;
				}

				case "join":
				{
					if (parts.Length != 3)
					{
						Writer.WriteLine ("channel join <id|name>");
						return true;
					}

					ChannelInfo channel = FindChannel (parts[2]);

					if (channel != null)
						Client.Users.Move (Client.CurrentUser, channel);
					else
						Writer.WriteLine ("Channel {0} not found.", parts[2]);

					return true;
				}

				case "move":
				{
					if (parts.Length != 4)
					{
						Writer.WriteLine ("channel move <userID|Name> <channelID|Name>");
						return true;
					}

					UserInfo user = FindUser (parts[2]);
					if (user == null)
					{
						Writer.WriteLine ("User {0} not found.", parts[2]);
						return true;
					}

					ChannelInfo channel = FindChannel (parts[3]);
					if (channel == null)
					{
						Writer.WriteLine ("Channel {0} not found.", parts[3]);
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