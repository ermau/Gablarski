﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Client;

namespace Gablarski.Clients.CLI
{
	public abstract class GClientModule
		: CommandModule
	{
		private readonly TextWriter writer;
		private readonly GablarskiClient client;

		protected GClientModule (GablarskiClient client, TextWriter writer)
		{
			this.client = client;
			this.writer = writer;
		}

		protected GablarskiClient Client
		{
			get { return this.client; }
		}

		protected TextWriter Writer
		{
			get { return this.writer; }
		}

		protected UserInfo FindUser (string part)
		{
			int userId;
			return (Int32.TryParse (part, out userId))
			       	? Client.Users[userId]
			       	: Client.Users.FirstOrDefault (u => u.Nickname.Trim().ToLower() == part.Trim().ToLower());
		}

		protected ChannelInfo FindChannel (string part)
		{
			int channelId;
			return (Int32.TryParse (part, out channelId))
			       	? Client.Channels[channelId]
			       	: Client.Channels.FirstOrDefault (c => c.Name.Trim().ToLower() == part.Trim().ToLower());
		}
	}
}