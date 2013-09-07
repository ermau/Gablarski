using System;
using System.Collections.Generic;
using System.Linq;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Messages;
using Tempest;

namespace Gablarski.Tests
{
	public class MockClientContext
		: TempestClient, IGablarskiClientContext
	{
		public MockClientContext (IClientConnection connection)
			: base (connection, MessageTypes.All, false)
		{
		}

		public IAudioEngine Audio { get; set; }

		/// <summary>
		/// Gets the channels in this context
		/// </summary>
		public IIndexedEnumerable<int, IChannelInfo> Channels { get; set; }

		/// <summary>
		/// Gets the source handler in this context
		/// </summary>
		public IClientSourceHandler Sources { get; set; }

		/// <summary>
		/// Gets the user associated with this context
		/// </summary>
		public IClientUserHandler Users { get; set; }

		/// <summary>
		/// Gets the current logged in user.
		/// </summary>
		public ICurrentUserHandler CurrentUser { get; set; }

		public ServerInfo ServerInfo { get; set; }
	}
}