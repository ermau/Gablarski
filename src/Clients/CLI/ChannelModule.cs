// Copyright (c) 2011, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

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

					IChannelInfo channel = FindChannel (parts[2]);

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

					IUserInfo user = FindUser (parts[2]);
					if (user == null)
					{
						Writer.WriteLine ("User {0} not found.", parts[2]);
						return true;
					}

					IChannelInfo channel = FindChannel (parts[3]);
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
		
		static void WriteChannels (IEnumerable<ChannelInfo> channels, TextWriter writer, ChannelInfo parent)
		{
			foreach (var c in channels.Where (ci => (parent != null) ? ci.ParentChannelId == parent.ChannelId : ci.ParentChannelId == 0))
			{
				WriteChannels (channels, writer, c);
			}
		}
	}
}