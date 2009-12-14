using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Client;

namespace Gablarski.Tests
{
	public class MockClientContext
		: IClientContext
	{
		#region Implementation of IClientContext

		public IAudioEngine Audio { get; set; }

		/// <summary>
		/// Gets the connection associated with this client.
		/// </summary>
		public IClientConnection Connection { get; set; }

		/// <summary>
		/// Gets the channels in this context
		/// </summary>
		public IIndexedEnumerable<int, ChannelInfo> Channels { get; set; }

		/// <summary>
		/// Gets teh source manager in this context
		/// </summary>
		public IIndexedEnumerable<int, AudioSource> Sources { get; set; }

		/// <summary>
		/// Gets the user associated with this context
		/// </summary>
		public IClientUserManager Users { get; set; }

		/// <summary>
		/// Gets the current logged in user.
		/// </summary>
		public CurrentUser CurrentUser { get; set; }

		public ServerInfo ServerInfo { get; set; }

		#endregion
	}
}