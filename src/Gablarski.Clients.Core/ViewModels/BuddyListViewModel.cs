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
using System.Windows.Input;
using Gablarski.Annotations;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Tempest.Social;

namespace Gablarski.Clients.ViewModels
{
	public class BuddyListViewModel
		: ViewModelBase
	{
		public BuddyListViewModel ([NotNull] GablarskiSocialClient client)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
			this.startChatCommand = new RelayCommand<Person> (StartChatCore, CanStartChatWithPerson);
		}

		public IEnumerable<Person> Buddies
		{
			get { return this.client.WatchList; }
		}

		public ICommand StartChat
		{
			get { return this.startChatCommand; }
		}

		private readonly GablarskiSocialClient client;
		private readonly RelayCommand<Person> startChatCommand;

		private bool CanStartChatWithPerson (Person parameter)
		{
			return true;
		}

		private async void StartChatCore (Person person)
		{
			await this.client.StartGroupWithAsync (person);
		}
	}
}