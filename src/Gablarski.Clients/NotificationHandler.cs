// Copyright (c) 2010, Eric Maupin
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
using Cadenza;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Clients.Media;

namespace Gablarski.Clients
{
	/// <summary>
	/// Handles automatically notifying a collection of <see cref="INotifier"/>s with the events.
	/// </summary>
	public class NotificationHandler
	{
		/// <summary>
		/// Creates a new instance of <see cref="NotificationHandler"/>.
		/// </summary>
		/// <param name="client"></param>
		public NotificationHandler (GablarskiClient client)
		{
			this.client = client;
			this.client.Disconnected += OnClientDisconnected;
			this.client.Users.UserJoined += OnUserJoined;
			this.client.Users.UserDisconnected += OnUserDisconnected;
			this.client.Users.UserChangedChannel += OnUserChangedChannel;
			this.client.Users.UserKickedFromChannel += OnUserKickedFromChannel;
			this.client.Users.UserKickedFromServer += OnUserKickedFromServer;
		}

		/// <summary>
		/// Gets or sets whether notifications are muted.
		/// </summary>
		public bool Muted
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets the <see cref="IAudioReceiver"/> that speech is coming through.
		/// </summary>
		/// <remarks>99.9% of the time, this will be <see cref="GablarskiClient.Sources"/>.</remarks>
		public IAudioReceiver SpeechReceiver
		{
			get;
			set;
		}

		/// <summary>
		/// Sets the speech notifiers.
		/// </summary>
		public IEnumerable<ITextToSpeech> SpeechNotifiers
		{
			set
			{
				lock (notifiers)
				{
					ClearSpeech();
					Attach (value);
				}
			}
		}

		/// <summary>
		/// Sets the notifiers to notify.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the <see cref="IMediaController"/> to use with the <see cref="SpeechNotifiers"/>.
		/// </summary>
		public IMediaController MediaController
		{
			get;
			set;
		}

		/// <summary>
		/// Sends <paramref name="contents"/> directly to the <see cref="SpeechNotifiers"/> only.
		/// </summary>
		/// <param name="contents">Well what do you want to say?</param>
		public void Say (string contents)
		{
			if (Muted || SpeechReceiver == null)
				return;

			lock (notifiers)
			{
				foreach (var n in speechNotifiers)
					SpeechReceiver.Receive (n.AudioSource, n.GetSpeech (contents, n.AudioSource));
			}
		}

		public void Notify (NotificationType type, string notification)
		{
			Notify (type, notification, NotifyPriority.Info);
		}

		public void Notify (NotificationType type, string notification, NotifyPriority priority)
		{
			if (Muted)
				return;

			lock (notifiers)
			{
				foreach (var n in notifiers)
					n.Notify (type, notification, priority);
			}

			if (SpeechReceiver != null)
			{
				lock (notifiers)
				{
					foreach (var n in speechNotifiers)
						SpeechReceiver.Receive (n.AudioSource, n.GetSpeech (notification, n.AudioSource));
				}
			}
		}

		public void Notify (NotificationType type, string notification, string nickname, string phonetic)
		{
			Notify (type, notification, nickname, phonetic, NotifyPriority.Info);
		}

		public void Notify (NotificationType type, string notification, string nickname, string phonetic, NotifyPriority priority)
		{
			if (Muted)
				return;

			lock (notifiers)
			{
				foreach (var n in notifiers)
					n.Notify (type, String.Format (notification, nickname), priority);
			}

			if (SpeechReceiver != null)
			{
				lock (notifiers)
				{
					foreach (var n in speechNotifiers)
						SpeechReceiver.Receive (n.AudioSource, n.GetSpeech (String.Format (notification, (!phonetic.IsNullOrWhitespace()) ? phonetic : nickname), n.AudioSource));
				}
			}
		}

		public void Close ()
		{
			Clear ();

			MediaController.Reset();

			this.client.Disconnected -= OnClientDisconnected;
			this.client.Users.UserJoined -= OnUserJoined;
			this.client.Users.UserDisconnected -= OnUserDisconnected;
			this.client.Users.UserChangedChannel -= OnUserChangedChannel;
		}

		private bool isDisposed;
		private readonly HashSet<INotifier> notifiers = new HashSet<INotifier> ();
		private readonly HashSet<ITextToSpeech> speechNotifiers = new HashSet<ITextToSpeech>();
		private readonly GablarskiClient client;

		private void OnUserKickedFromServer (object sender, UserEventArgs e)
		{
			Notify (NotificationType.UserKicked, "{0} was kicked from the server.", e.User.Nickname, e.User.Phonetic, NotifyPriority.Important);
		}

		private void OnUserKickedFromChannel (object sender, UserEventArgs e)
		{
			if (e.User.CurrentChannelId == client.CurrentUser.CurrentChannelId)
				Notify (NotificationType.UserKicked, "{0} was kicked from the channel.", e.User.Nickname, e.User.Phonetic, NotifyPriority.Important);
		}

		private void OnClientDisconnected (object sender, EventArgs e)
		{
			Notify (NotificationType.Disconnected, "Disconnected.");
		}

		private void OnUserDisconnected (object sender, UserEventArgs e)
		{
			if (!e.User.Equals (client.CurrentUser))
				Notify (NotificationType.UserLeftServer, "{0} has left the server.", e.User.Nickname, e.User.Phonetic);
		}

		private void OnUserJoined (object sender, UserEventArgs e)
		{
			if (!e.User.Equals (client.CurrentUser))
				Notify (NotificationType.UserJoinedServer, "{0} has joined the server.", e.User.Nickname, e.User.Phonetic);
		}

		private void OnUserChangedChannel (object sender, ChannelChangedEventArgs e)
		{
			if (e.User.Equals (client.CurrentUser))
				Notify (NotificationType.SwitchedChannel, "Switched to channel " + e.TargetChannel.Name + ".");
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

		private void Attach (IEnumerable<INotifier> attachNotifiers)
		{
			foreach (var n in attachNotifiers)
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

		private void ClearSpeech()
		{
			lock (notifiers)
			{
				foreach (var n in speechNotifiers.ToList())
					Detatch (n);
			}
		}

		private void Attach (IEnumerable<ITextToSpeech> sNotifiers)
		{
			lock (notifiers)
			{
				foreach (var n in sNotifiers)
					Attach (n);
			}
		}

		private void Attach (ITextToSpeech notifier)
		{
			lock (notifiers)
				this.speechNotifiers.Add (notifier);
		}

		private void Detatch (ITextToSpeech notifier)
		{
			lock (notifier)
			{
				notifier.Media = null;
				this.speechNotifiers.Remove (notifier);
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