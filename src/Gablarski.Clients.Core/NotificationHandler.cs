// Copyright (c) 2009-2014, Eric Maupin
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
using Cadenza.Collections;
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
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
			this.client.Disconnected += OnClientDisconnected;
			this.client.Users.UserJoined += OnUserJoined;
			this.client.Users.UserDisconnected += OnUserDisconnected;
			this.client.Users.UserChangedChannel += OnUserChangedChannel;
			this.client.Users.UserKicked += OnUserKicked;
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
		/// Gets or sets the <see cref="IMediaController"/> to use with the speech notifiers.
		/// </summary>
		public IMediaController MediaController
		{
			get;
			set;
		}

		/// <summary>
		/// Adds a notifier.
		/// </summary>
		/// <param name="notifier">The notifier to add.</param>
		/// <param name="enabledNotifications">The notifications to enable for this <paramref name="notifier"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="notifier"/> or <paramref name="enabledNotifications"/> is <c>null</c>.</exception>
		public void AddNotifier (INotifier notifier, IEnumerable<NotificationType> enabledNotifications)
		{
			if (notifier == null)
				throw new ArgumentNullException ("notifier");
			if (enabledNotifications == null)
				throw new ArgumentNullException ("enabledNotifications");

			lock (this.notifications)
			{
				foreach (NotificationType type in enabledNotifications)
					this.notifications.Add (type, notifier);
			}
		}

		/// <summary>
		/// Adds a notifier.
		/// </summary>
		/// <param name="notifier">The notifier to add.</param>
		/// <param name="enabledNotifications">The notifications to enable for this <paramref name="notifier"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="notifier"/> or <paramref name="enabledNotifications"/> is <c>null</c>.</exception>
		public void AddNotifier (ITextToSpeech notifier, IEnumerable<NotificationType> enabledNotifications)
		{
			if (notifier == null)
				throw new ArgumentNullException ("notifier");
			if (enabledNotifications == null)
				throw new ArgumentNullException ("enabledNotifications");

			lock (this.speechNotifiers)
			{
				foreach (NotificationType type in enabledNotifications)
					this.speechNotifiers.Add (type, notifier);
			}
		}

		/// <summary>
		/// Removes the notifier.
		/// </summary>
		/// <param name="notifier">The notifier to remove.</param>
		/// <returns><c>true</c> if <paramref name="notifier"/> was found and removed.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="notifier"/> is <c>null</c>.</exception>
		public bool RemoveNotifier (INotifier notifier)
		{
			if (notifier == null)
				throw new ArgumentNullException ("notifier");
			
			bool found = false;

			lock (this.notifications)
			{
				foreach (var g in new MutableLookup<NotificationType, INotifier> (this.notifications).Where (g => g.Contains (notifier)))
				{
					if (this.notifications.Remove (g.Key, notifier))
						found = true;
				}
			}

			return found;
		}

		/// <summary>
		/// Removes the notifier.
		/// </summary>
		/// <param name="notifier">The notifier to remove.</param>
		/// <returns><c>true</c> if <paramref name="notifier"/> was found and removed.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="notifier"/> is <c>null</c>.</exception>
		public bool RemoveNotifier (ITextToSpeech notifier)
		{
			if (notifier == null)
				throw new ArgumentNullException ("notifier");

			bool found = false;

			lock (this.speechNotifiers)
			{
				foreach (var g in new MutableLookup<NotificationType, ITextToSpeech> (this.speechNotifiers).Where (g => g.Contains (notifier)))
				{
					if (this.speechNotifiers.Remove (g.Key, notifier))
						found = true;
				}
			}

			return found;
		}

		/// <summary>
		/// Clears all notifiers.
		/// </summary>
		public void Clear()
		{
			lock (this.notifications)
				this.notifications.Clear();

			lock (this.speechNotifiers)
				this.speechNotifiers.Clear();
		}

		public void Notify (NotificationType type, string notification)
		{
			Notify (type, notification, NotifyPriority.Info);
		}

		public void Notify (NotificationType type, string notification, NotifyPriority priority)
		{
			if (Muted)
				return;

			lock (notifications)
			{
				IEnumerable<INotifier> notifiers;
				if (this.notifications.TryGetValues (type, out notifiers))
				{
					foreach (var n in notifiers)
						n.Notify (type, notification, priority);
				}
			}

			if (SpeechReceiver != null)
			{
				lock (speechNotifiers)
				{
					IEnumerable<ITextToSpeech> speakers;
					if (this.speechNotifiers.TryGetValues (type, out speakers))
					{
						foreach (var n in speakers)
							SpeechReceiver.Receive (n.AudioSource, n.GetSpeech (notification));
					}
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

			lock (notifications)
			{
				IEnumerable<INotifier> notifiers;
				if (this.notifications.TryGetValues (type, out notifiers))
				{
					foreach (var n in notifiers)
						n.Notify (type, String.Format (notification, nickname), priority);
				}
			}

			if (SpeechReceiver != null)
			{
				lock (speechNotifiers)
				{
					IEnumerable<ITextToSpeech> speakers;
					if (this.speechNotifiers.TryGetValues (type, out speakers))
					{
						foreach (var n in speakers)
							SpeechReceiver.Receive (n.AudioSource, n.GetSpeech (String.Format (notification, (!phonetic.IsNullOrWhitespace()) ? phonetic : nickname)));
					}
				}
			}
		}

		public void Close ()
		{
			Clear ();

			if (MediaController != null)
				MediaController.Reset();

			this.client.Disconnected -= OnClientDisconnected;
			this.client.Users.UserJoined -= OnUserJoined;
			this.client.Users.UserDisconnected -= OnUserDisconnected;
			this.client.Users.UserChangedChannel -= OnUserChangedChannel;
			this.client.Users.UserKicked -= OnUserKicked;
		}

		private bool isDisposed;
		private readonly MutableLookup<NotificationType, INotifier> notifications = new MutableLookup<NotificationType, INotifier>();
		private readonly MutableLookup<NotificationType, ITextToSpeech> speechNotifiers = new MutableLookup<NotificationType, ITextToSpeech>();
		private readonly GablarskiClient client;

		private void OnUserKicked (object sender, UserKickedEventArgs e)
		{
			if (e.FromServer)
				Notify (NotificationType.UserKicked, "{0} was kicked from the server.", e.User.Nickname, e.User.Phonetic, NotifyPriority.Important);
			else if (e.Channel.ChannelId == client.CurrentUser.CurrentChannelId)
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
			else if (e.TargetChannel.Equals (client.Channels.Current))
				Notify (NotificationType.UserJoinedChannel, "{0} joined the channel.", e.User.Nickname, e.User.Phonetic);
			else if (e.PreviousChannel.Equals (client.Channels.Current))
				Notify (NotificationType.UserLeftChannel, "{0} left the channel.", e.User.Nickname, e.User.Phonetic);
		}
	}
}