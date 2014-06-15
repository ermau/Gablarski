//
// ServerEntryViewModel.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2014 Xamarin Inc.
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
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Gablarski.Client;
using Gablarski.Clients.Messages;
using Gablarski.Clients.Persistence;
using Tempest;
using Timer = System.Threading.Timer;

namespace Gablarski.Clients.ViewModels
{
	public class ServerEntryViewModel
		: ViewModelBase, INotifyDataErrorInfo
	{
		private static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds (15);

		public ServerEntryViewModel (RSAAsymmetricKey key, ServerEntry server)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (server == null)
				throw new ArgumentNullException ("server");

			this.key = key;

			Server = new ServerEntry (server);

			this.host = server.Host;
			this.port = server.Port;
			this.name = server.Name;
			this.nickname = server.UserNickname;

			KickOffQuery();

			this.save = new RelayCommand (OnSave, CanSave);
			Cancel = new RelayCommand (() => Messenger.Send (new DoneEditingServerMessage()));
		}

		public ServerEntryViewModel (RSAAsymmetricKey key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			this.key = key;

			Server = new ServerEntry (0) {
				Port = 42912
			};
		}

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
		{
			add { this.errors.ErrorsChanged += value; }
			remove { this.errors.ErrorsChanged -= value; }
		}

		public bool HasErrors
		{
			get { return this.errors.HasErrors; }
		}

		public AsyncValue<bool?> IsOnline
		{
			get { return this.isOnline; }
			private set
			{
				if (this.isOnline == value)
					return;

				this.isOnline = value; 
				OnPropertyChanged();
			}
		}

		public AsyncValue<int> ConnectedUsers
		{
			get { return this.connectedUsers; }
			private set
			{
				if (this.connectedUsers == value)
					return;

				this.connectedUsers = value;
				OnPropertyChanged();
			}
		}

		public string Host
		{
			get { return this.host; }
			set
			{
				if (this.host == value)
					return;

				this.errors.ClearErrors();

				this.host = value;
				OnPropertyChanged();

				if (String.IsNullOrWhiteSpace (value))
					this.errors.AddError ("Hostname must be filled in");

				this.save.RaiseCanExecuteChanged();

				KickOffQuery();
			}
		}

		public int Port
		{
			get { return this.port; }
			set
			{
				if (this.port == value)
					return;

				this.errors.ClearErrors();

				this.port = value;
				OnPropertyChanged();

				if (value < 0 || value > UInt16.MaxValue)
					this.errors.AddError ("Invalid port");

				this.save.RaiseCanExecuteChanged();

				KickOffQuery();
			}
		}

		public string Name
		{
			get { return this.name; }
			set
			{
				if (this.name == value)
					return;

				this.errors.ClearErrors();

				this.name = value;
				OnPropertyChanged();

				if (String.IsNullOrWhiteSpace (value))
					this.errors.AddError ("Server name must be filled in");

				this.save.RaiseCanExecuteChanged();
			}
		}

		public string Nickname
		{
			get { return this.nickname; }
			set
			{
				if (this.nickname == value)
					return;

				this.errors.ClearErrors();

				this.nickname = value;
				OnPropertyChanged();

				if (String.IsNullOrWhiteSpace (value))
					this.errors.AddError ("You must enter a nickname");

				this.save.RaiseCanExecuteChanged();
			}
		}

		public ICommand Save
		{
			get { return this.save; }
		}

		public ICommand Cancel
		{
			get;
			private set;
		}

		public ServerEntry Server
		{
			get;
			private set;
		}

		public IEnumerable GetErrors (string propertyName)
		{
			return this.errors.GetErrors (propertyName);
		}

		private int port;
		private string host, name, nickname;

		private readonly RSAAsymmetricKey key;
		private Timer requeryTimer;
		private AsyncValue<bool?> isOnline;
		private AsyncValue<int> connectedUsers;
		private readonly ErrorManager errors = new ErrorManager();
		private RelayCommand save;

		private bool GetHostAndPortValid()
		{
			return GetHostAndPortValid (Host, Port);
		}

		private bool GetHostAndPortValid (string host, int port)
		{
			return (!String.IsNullOrWhiteSpace (host)) && (port > 0 && port < UInt16.MaxValue);
		}

		private bool CanSave()
		{
			return (!HasErrors && GetHostAndPortValid() && !String.IsNullOrWhiteSpace (Name) &&
			        !String.IsNullOrWhiteSpace (Nickname));
		}

		private void OnSave()
		{
			var entry = new ServerEntry (Server) {
				Name = this.name,
				Host = this.host,
				Port = this.port,
				UserNickname = this.nickname
			};

			ClientData.SaveOrUpdate (entry);

			Messenger.Send (new DoneEditingServerMessage());
		}

		private void Query()
		{
			int p = Server.Port;
			string h = Server.Host;
			if (!GetHostAndPortValid (h, p))
				return;

			Task<QueryResults> query = GablarskiClient.QueryAsync (key, new Target (h, p), QueryTimeout);

			query.ContinueWith (t => {
				if (!String.IsNullOrWhiteSpace (Name))
					return;

				Name = t.Result.ServerInfo.Name;
			}, TaskContinuationOptions.OnlyOnRanToCompletion);

			IsOnline = new AsyncValue<bool?> (query.ContinueWith (t => (bool?) (!t.IsCanceled), TaskContinuationOptions.HideScheduler), null, false);
			ConnectedUsers = new AsyncValue<int> (query.ContinueWith (t => t.Result.Users.Count(), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.HideScheduler), 0);
		}

		private void KickOffQuery ()
		{
			if (this.requeryTimer != null)
				this.requeryTimer.Dispose();

			if (!GetHostAndPortValid())
				return;

			this.requeryTimer = new Timer (o => Query(), null, 0, 30000);
		}
	}
}