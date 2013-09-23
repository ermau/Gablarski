// Copyright (c) 2013, Eric Maupin
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Gablarski.Messages;
using Tempest;
using Tempest.Social;
using JoinVoiceMessage = Gablarski.Clients.Messages.JoinVoiceMessage;

namespace Gablarski.Clients.ViewModels
{
	public class GroupViewModel
		: ViewModelBase
	{
		public GroupViewModel (GablarskiSocialClient client, Group group, IEnumerable<TextMessage> chatLog)
		{
			if (client == null)
				throw new ArgumentNullException ("client");
			if (group == null)
				throw new ArgumentNullException ("group");
			if (chatLog == null)
				throw new ArgumentNullException ("chatLog");

			Group = group;
			this.client = client;
			Chat = chatLog;

			INotifyCollectionChanged change = (INotifyCollectionChanged)group.Participants;
			change.CollectionChanged += OnParticipantsChanged;

			SendTextMessage = new RelayCommand<TextMessage> (OnSendTextMessage);

			JoinVoice = new RelayCommand (OnJoinVoice);
		}

		public Group Group
		{
			get;
			private set;
		}

		public bool IsOwner
		{
			get { return this.client.Persona.Identity == Group.OwnerId; }
		}

		public IEnumerable<TextMessage> Chat
		{
			get;
			private set;
		}

		public ICommand JoinVoice
		{
			get;
			private set;
		}

		public ICommand SendTextMessage
		{
			get;
			private set;
		}

		public IEnumerable<Person> Participants
		{
			get
			{
				foreach (string id in Group.Participants) {
					Person person;
					if (!this.client.WatchList.TryGetPerson (id, out person))
						person = new Person (id) { Nickname = id }; // HACK

					yield return person;
				}
			}
		}

		private readonly GablarskiSocialClient client;

		private void OnParticipantsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged ("Participants");
		}

		private async void OnSendTextMessage (TextMessage textMessage)
		{
			await client.SendTextAsync (textMessage.Group, textMessage.Message);
		}

		private async void OnJoinVoice()
		{
			Target target;
			if (IsOwner) {
				await LocalServer.StartAsync (this.client.Connection.LocalKey);
				target = new Target (Target.LoopbackIP, GablarskiProtocol.Port);
			} else {
				try {
					target = await this.client.RequestGroupVoiceAsync (Group);
				} catch (OperationCanceledException) {
					return;
				}
			}

			Messenger.Send (new JoinVoiceMessage (target));
		}
	}
}