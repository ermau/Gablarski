using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Client;

namespace Gablarski
{
	/// <summary>
	/// Represents a gablarski client context.
	/// </summary>
	public interface IClientContext
	{
		IAudioEngine Audio { get; }

		/// <summary>
		/// Gets the connection associated with this client.
		/// </summary>
		IClientConnection Connection { get; }

		/// <summary>
		/// Gets the channels in this context
		/// </summary>
		IIndexedEnumerable<int, ChannelInfo> Channels { get; }

		/// <summary>
		/// Gets the sources in this context.
		/// </summary>
		IIndexedEnumerable<int, AudioSource> Sources { get; }

		/// <summary>
		/// Gets the user associated with this context
		/// </summary>
		IIndexedEnumerable<int, ClientUser> Users { get; }

		/// <summary>
		/// Gets the current logged in user.
		/// </summary>
		CurrentUser CurrentUser { get; }
	}
}