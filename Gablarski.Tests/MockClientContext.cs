using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;
using Gablarski.Media.Sources;

namespace Gablarski.Tests
{
	public class MockClientContext
		: IClientContext
	{
		#region Implementation of IClientContext

		/// <summary>
		/// Gets the connection associated with this client.
		/// </summary>
		public IClientConnection Connection { get; set; }

		/// <summary>
		/// Gets the channels in this context
		/// </summary>
		public IEnumerable<Channel> Channels { get; set; }

		/// <summary>
		/// Gets teh source manager in this context
		/// </summary>
		public IEnumerable<MediaSourceBase> Sources { get; set; }

		/// <summary>
		/// Gets the user associated with this context
		/// </summary>
		public IEnumerable<ClientUser> Users { get; set; }

		#endregion
	}
}