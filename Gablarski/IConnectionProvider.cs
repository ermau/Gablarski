// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

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

namespace Gablarski
{
	public interface IConnectionProvider
	{
		/// <summary>
		/// A connectionless message was received.
		/// </summary>
		event EventHandler<MessageReceivedEventArgs> ConnectionlessMessageReceived;

		/// <summary>
		/// A connection was made.
		/// </summary>
		event EventHandler<ConnectionEventArgs> ConnectionMade;

		/// <summary>
		/// Starts listening for connections and connectionless messages.
		/// </summary>
		void StartListening ();

		/// <summary>
		/// Stops listening for connections and connectionless messages.
		/// </summary>
		void StopListening ();
	}

	/// <summary>
	/// Provides data for the <see cref="IConnectionProvider.ConnectionMade"/> event.
	/// </summary>
	public class ConnectionEventArgs
		: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectionEventArgs"/> class.
		/// </summary>
		/// <param name="connection">The connection that was made.</param>
		public ConnectionEventArgs (IConnection connection)
		{
			this.Connection = connection;
		}

		/// <summary>
		/// Gets the connection made.
		/// </summary>
		public IConnection Connection
		{
			get;
			private set;
		}
	}
}