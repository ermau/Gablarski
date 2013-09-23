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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tempest.Social;
using TextMessage = Gablarski.Clients.ViewModels.TextMessage;

namespace Gablarski.Clients
{
	public sealed class ChatHistory
	{
		private readonly SocialClient socialClient;

		public ChatHistory (SocialClient socialClient)
		{
			if (socialClient == null)
				throw new ArgumentNullException ("socialClient");

			this.socialClient = socialClient;
			this.socialClient.ReceivedTextMessage += OnReceivedTextMessage;
		}

		/// <remarks>
		/// Lock on the returned enumerable.
		/// </remarks>
		public IEnumerable<TextMessage> GetMessages (Group group)
		{
			if (group == null)
				throw new ArgumentNullException ("group");

			return GetMessageCollection (group);
		}

		private readonly ConcurrentDictionary<Group, ObservableCollection<TextMessage>> messages = new ConcurrentDictionary<Group, ObservableCollection<TextMessage>>();

		private ObservableCollection<TextMessage> GetMessageCollection (Group group)
		{
			ObservableCollection<TextMessage> groupMessages;
			if (!messages.TryGetValue (group, out groupMessages)) {
				messages.TryAdd (group, new ObservableCollection<TextMessage>());
				messages.TryGetValue (group, out groupMessages);
			}

			return groupMessages;
		}

		private void OnReceivedTextMessage (object sender, TextMessageEventArgs e)
		{
			var groupMessages = GetMessageCollection (e.Group);

			lock (groupMessages) {
				groupMessages.Add (new TextMessage (e.Group, e.Person, e.Message));
			}
		}
	}
}
