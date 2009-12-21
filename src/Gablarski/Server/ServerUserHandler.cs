// Copyright (c) 2009, Eric Maupin
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
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Server
{
	public class ServerUserHandler
		: IServerUserHandler
	{
		public ServerUserHandler (IServerContext context, IServerUserManager manager)
		{
			this.serverContext = context;
			this.Manager = manager;
		}

		#region IIndexedEnumerable<int,UserInfo> Members

		public UserInfo this[int key]
		{
			get { return Manager[key]; }
		}

		#endregion

		#region IEnumerable<UserInfo> Members

		public IEnumerator<UserInfo> GetEnumerator()
		{
			return Manager.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		internal readonly IServerUserManager Manager;
		private readonly IServerContext serverContext;

		internal void ConnectMessage (MessageReceivedEventArgs e)
		{
			var msg = (ConnectMessage)e.Message;

			if (msg.ProtocolVersion < serverContext.ProtocolVersion)
			{
				e.Connection.Send (new ConnectionRejectedMessage (ConnectionRejectedReason.IncompatibleVersion));
				return;
			}

			Manager.Connect (e.Connection);
			e.Connection.Send (new ServerInfoMessage { ServerInfo = new ServerInfo (serverContext.Settings) });
		}
	}
}