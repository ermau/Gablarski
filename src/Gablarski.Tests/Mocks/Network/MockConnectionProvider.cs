// Copyright (c) 2010, Eric Maupin
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
using System.Linq;
using System.Net;
using Gablarski.Server;
using Gablarski.Messages;

namespace Gablarski.Tests
{
	public class MockConnectionProvider
		: IConnectionProvider
	{
		public MockServerConnection EstablishConnection ()
		{
			return EstablishConnection (null);
		}

		public MockServerConnection EstablishConnection (IPAddress ipAddress)
		{
			var connection = new MockServerConnection ();
			connection.IPAddress = ipAddress;

			var c = new ConnectionMadeEventArgs (connection);

			var connectionMade = this.ConnectionMade;
			if (connectionMade != null)
				connectionMade (this, c);

			if (c.Cancel)
				return null;

			return connection;
		}

		public bool IsListening
		{
			get;
			private set;
		}

		#region IConnectionProvider Members

		public event EventHandler<ConnectionMadeEventArgs> ConnectionMade;

		public event EventHandler<ConnectionlessMessageReceivedEventArgs> ConnectionlessMessageReceived;

		/// <summary>
		/// Sends a connectionless <paramref name="message"/> to the <paramref name="endpoint"/>.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <param name="endpoint">The endpoint to send the <paramref name="message"/> to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="message"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="endpoint"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="message"/> is set as a reliable message.</exception>
		/// <seealso cref="IConnectionProvider.ConnectionlessMessageReceived"/>
		public void SendConnectionlessMessage (MessageBase message, EndPoint endpoint)
		{
		}

		public void StartListening (IServerContext serverContext)
		{
			this.IsListening = true;
		}

		public void StopListening ()
		{
			this.IsListening = false;
		}

		#endregion
	}
}