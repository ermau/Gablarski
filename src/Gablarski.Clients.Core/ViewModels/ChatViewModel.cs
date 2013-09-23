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
using System.Linq;
using System.Windows.Input;
using Gablarski.Annotations;
using Tempest.Social;

namespace Gablarski.Clients.ViewModels
{
	public class ChatViewModel
		: ViewModelBase
	{
		private readonly GablarskiSocialClient client;

		public ChatViewModel ([NotNull] GablarskiSocialClient client, ChatHistory history)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
			this.closeGroupCommand = new RelayCommand<Group> (CloseGroupCore);
			this.groups = new SelectedObservableCollection<Group, GroupViewModel> (client.Groups,
				g => new GroupViewModel (client, g, history.GetMessages (g)));
		}

		public IEnumerable<GroupViewModel> Groups
		{
			get { return this.groups; }
		}

		private SelectedObservableCollection<Group, GroupViewModel> groups;
		private readonly RelayCommand<Group> closeGroupCommand;

		public ICommand CloseGroup
		{
			get { return this.closeGroupCommand; }
		}

		private void CloseGroupCore (Group group)
		{
			GroupViewModel vm = this.groups.FirstOrDefault (g => g.Group.Id == group.Id);
			if (vm == null)
				return;

			this.groups.Remove (vm);
		}
	}
}
