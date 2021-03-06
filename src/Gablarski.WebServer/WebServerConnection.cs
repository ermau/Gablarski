﻿// Copyright (c) 2010, Eric Maupin
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
using System.Linq;
using System.Net;
using Gablarski.Messages;
using Gablarski.Server;
using HttpServer.Sessions;

namespace Gablarski.WebServer
{
	public class WebServerConnection
		: IServerConnection
	{
		public WebServerConnection (IHttpSession session, IPAddress ipAddress)
		{
			if (session == null)
				throw new ArgumentNullException ("session");
			if (ipAddress == null)
				throw new ArgumentNullException ("ipAddress");

			IPAddress = ipAddress;
			this.IsConnected = true;
			this.session = session;
		}

		#region Implementation of IConnection
		public event EventHandler<MessageReceivedEventArgs> MessageReceived;
		public event EventHandler<ConnectionEventArgs> Disconnected;

		/// <summary>
		/// Gets whether the connection is active.
		/// </summary>
		public bool IsConnected
		{
			get; private set;
		}

		public bool IsAsync
		{
			get { return true; }
		}

		public IPAddress IPAddress
		{
			get;
			private set;
		}

		public IEncryption Encryption
		{
			get; set;
		}

		public IDecryption Decryption
		{
			get; set;
		}

			/// <summary>
		/// Sends <paramref name="message"/> to the other end of the connection.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="message"/> is <c>null</c>.</exception>
		public void Send (MessageBase message)
		{
			var mqueue = ((List<MessageBase>)session["mqueue"]);
			lock (mqueue)
				mqueue.Add (message);
		}

		public IEnumerable<ReceivedMessage> Tick()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		public void Disconnect()
		{
			this.IsConnected = false;
		}

		public void DisconnectAsync()
		{
			Disconnect();
		}

		#endregion

		private readonly IHttpSession session;

		internal void Receive (MessageBase messageBase)
		{
			var received = this.MessageReceived;
			if (received != null)
				received (this, new MessageReceivedEventArgs (this, messageBase));
		}
	}
}
