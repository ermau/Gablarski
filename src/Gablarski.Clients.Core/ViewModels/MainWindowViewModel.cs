// Copyright (c) 2012-2013, Eric Maupin
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
using System.Windows.Input;
using Gablarski.Annotations;
using Gablarski.Clients.Messages;
using Gablarski.Clients.Persistence;
using Tempest.Social;

namespace Gablarski.Clients.ViewModels
{
	public class MainWindowViewModel
		: ViewModelBase
	{
		public MainWindowViewModel ([NotNull] GablarskiSocialClient client)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
			this.buddyListViewModel = new BuddyListViewModel (client);
			this.onlineFilter = new ObservableFilter<Person, WatchList> (this.client.WatchList, p => p.Status != Status.Offline);

			Servers = new AsyncValue<IEnumerable<ServerEntry>> (ClientData.GetServersAsync(), Enumerable.Empty<ServerEntry>());

			AddBuddy = new RelayCommand (() => Messenger.Send (new AddBuddyMessage()));
			StartChat = new RelayCommand<Person> (OnStartChat);
		}

		public Person Persona
		{
			get { return this.client.Persona; }
		}

		public BuddyListViewModel BuddyList
		{
			get { return this.buddyListViewModel; }
		}

		public AsyncValue<IEnumerable<ServerEntry>> Servers
		{
			get;
			private set;
		}

		public ICommand AddBuddy
		{
			get;
			private set;
		}

		public ICommand StartChat
		{
			get;
			private set;
		}

		public ICommand InviteToChat
		{
			get;
			private set;
		}

		private readonly ObservableFilter<Person, WatchList> onlineFilter;
		private readonly GablarskiSocialClient client;
		private readonly BuddyListViewModel buddyListViewModel;

		private async void OnStartChat (Person person)
		{
			if (person == null)
				return;

			Group group = await this.client.StartGroupWithAsync (person);
			if (group == null)
				return;

			Messenger.Send (new StartChatMessage (group));
		}
	}
}