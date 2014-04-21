//
// ServerListViewModel.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2014, Eric Maupin
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
using Gablarski.Client;
using Gablarski.Clients.Messages;
using Gablarski.Clients.Persistence;
using Tempest;

namespace Gablarski.Clients.ViewModels
{
	public class ServerListViewModel
		: ViewModelBase
	{
		private readonly RSAAsymmetricKey key;
		private readonly IntPtr windowHandle;
		private ServerViewModel currentServer;

		public ServerListViewModel (RSAAsymmetricKey key, IntPtr windowHandle)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			this.key = key;
			this.windowHandle = windowHandle;
			Servers = new AsyncValue<IEnumerable<ServerEntryViewModel>> (
				ClientData.GetServersAsync().ContinueWith (t => t.Result.Select (se => new ServerEntryViewModel (key, se))),
				Enumerable.Empty<ServerEntryViewModel>());

			Messenger.Register<JoinServerMessage> (OnJoinServerMessage);
		}

		public AsyncValue<IEnumerable<ServerEntryViewModel>> Servers
		{
			get;
			private set;
		}

		public ServerViewModel CurrentServer
		{
			get { return this.currentServer; }
			private set
			{
				if (this.currentServer == value)
					return;

				this.currentServer = value;
				OnPropertyChanged();
			}
		}

		private async void OnJoinServerMessage (JoinServerMessage msg)
		{
			if (CurrentServer != null)
				throw new InvalidOperationException ("Can not join a server while one is already active");

			var client = new GablarskiClient (this.key);
			CurrentServer = new ServerViewModel (client, this.windowHandle);

			await CurrentServer.JoinAsync (msg.Server);
		}
	}
}