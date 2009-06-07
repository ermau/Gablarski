using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Gablarski
{
	/// <summary>
	/// Contract for a client -> server connection.
	/// </summary>
	public interface IClientConnection
		: IConnection
	{
		/// <summary>
		/// The client has succesfully connected to the end point.
		/// </summary>
		event EventHandler Connected;

		/// <summary>
		/// Connects to <paramref name="host"/>:<paramref name="port"/>.
		/// </summary>
		/// <param name="host">The host to connect to.</param>
		/// <param name="port">The port at which to connect to.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="host"/> is over 126 characters.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="port"/> is not between <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
		void Connect (string host, int port);

		/// <summary>
		/// Connects to <paramref name="endpoint"/>.
		/// </summary>
		/// <param name="endpoint">The endpoint to connect to.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="endpoint"/> is <c>null</c>.</exception>
		void Connect (IPEndPoint endpoint);
	}
}