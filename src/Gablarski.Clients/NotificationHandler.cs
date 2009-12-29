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
using Gablarski.Client;
using Gablarski.Clients.Media;

namespace Gablarski.Clients
{
	public class NotificationHandler
	{
		public NotificationHandler (GablarskiClient client)
		{
			this.client = client;
			this.client.Connected += OnClientConnected;
			this.client.Disconnected += OnClientDisconnected;
			this.client.Users.UserJoined += OnUserJoined;
			this.client.Users.UserDisconnected += OnUserDisconnected;
			this.client.Users.UserChangedChannel += OnUserChangedChannel;
		}

		public IEnumerable<INotifier> Notifiers
		{
			set
			{
				lock (notifiers)
				{
					Clear ();
					Attach (value);
				}
			}
		}

		public IMediaController MediaController
		{
			get;
			set;
		}

		public void Notify (NotificationType type, string notification)
		{
			Notify (type, notification, NotifyPriority.Info);
		}

		public void Notify (NotificationType type, string notification, NotifyPriority priority)
		{
			lock (notifiers)
			{
				foreach (var n in notifiers)
					n.Notify (type, notification, priority);
			}
		}

		public void Notify (NotificationType type, string notification, string nickname, string phonetic)
		{
			Notify (type, notification, nickname, phonetic, NotifyPriority.Info);
		}

		public void Notify (NotificationType type, string notification, string nickname, string phonetic, NotifyPriority priority)
		{
			lock (notifiers)
			{
				foreach (var n in notifiers)
					n.Notify (type, notification, nickname, phonetic, priority);
			}
		}

		public void Close ()
		{
			Clear ();

			MediaController.Reset();

			this.client.Connected -= OnClientConnected;
			this.client.Disconnected -= OnClientDisconnected;
			this.client.Users.UserJoined -= OnUserJoined;
			this.client.Users.UserDisconnected -= OnUserDisconnected;
			this.client.Users.UserChangedChannel -= OnUserChangedChannel;
		}

		private readonly HashSet<INotifier> notifiers = new HashSet<INotifier> ();
		private readonly GablarskiClient client;

		private void OnClientDisconnected (object sender, EventArgs e)
		{
			Notify (NotificationType.Disconnected, "Disconnected");
		}

		private void OnClientConnected (object sender, EventArgs e)
		{
			Notify (NotificationType.Connected, "Connected");
		}

		private void OnUserDisconnected (object sender, UserEventArgs e)
		{
			if (!e.User.Equals (client.CurrentChannel))
				Notify (NotificationType.UserLeftServer, "{0} has left the server.", e.User.Nickname, e.User.Phonetic);
		}

		private void OnUserJoined (object sender, UserEventArgs e)
		{
			if (!e.User.Equals (client.CurrentUser))
				Notify (NotificationType.UserJoinedServer, "{0} has joined the server.", e.User.Nickname, e.User.Phonetic);
		}

		void OnUserChangedChannel (object sender, ChannelChangedEventArgs e)
		{
			if (e.User.Equals (client.CurrentUser))
				Notify (NotificationType.SwitchedChannel, "Switched to channel " + e.TargetChannel.Name);
			else if (e.TargetChannel.Equals (client.CurrentChannel))
				Notify (NotificationType.UserJoinedChannel, "{0} joined the channel.", e.User.Nickname, e.User.Phonetic);
			else if (e.PreviousChannel.Equals (client.CurrentChannel))
				Notify (NotificationType.UserLeftChannel, "{0} left the channel.", e.User.Nickname, e.User.Phonetic);
		}

		private void Attach (INotifier notifier)
		{
			lock (notifiers)
			{
				notifier.Media = this.MediaController;

				if (!notifiers.Contains (notifier))
					notifiers.Add (notifier);
			}
		}

		private void Attach (IEnumerable<INotifier> notifiers)
		{
			foreach (var n in notifiers)
				Attach (n);
		}

		private void Detatch (INotifier notifier)
		{
			lock (notifiers)
			{
				notifier.Media = null;
				this.notifiers.Remove (notifier);
			}
		}

		private void Clear ()
		{
			lock (notifiers)
			{
				foreach (var n in notifiers.ToList ())
					Detatch (n);
			}
		}
	}
}