using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;
using Gablarski.Media.Sources;

namespace Gablarski
{
	/// <summary>
	/// Represents a gablarski client context.
	/// </summary>
	public interface IClientContext
	{
		/// <summary>
		/// Gets the connection associated with this client.
		/// </summary>
		IClientConnection Connection { get; }

		/// <summary>
		/// Gets the channels in this context
		/// </summary>
		IEnumerable<Channel> Channels { get; }

		/// <summary>
		/// Gets teh source manager in this context
		/// </summary>
		IEnumerable<MediaSourceBase> Sources { get; }

		/// <summary>
		/// Gets the user associated with this context
		/// </summary>
		IEnumerable<ClientUser> Users { get; }
	}
}