using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Synthesis;
using Gablarski.Client;
using Gablarski.Clients.Music;
using System.Threading;

namespace Gablarski.Clients.Windows
{
	public class EventSpeech
	{
		public EventSpeech (GablarskiClient client, MediaPlayerIntegration media)
		{
			this.media = media;
			this.client = client;
			this.client.Users.UserJoined += OnUserJoined;
			this.client.Users.UserDisconnected += OnUserDisconnected;
			this.client.Users.UserChangedChannel += new EventHandler<ChannelChangedEventArgs> (OnUserChangedChannel);
		}

		public void Detatch ()
		{
			this.client.Users.UserJoined -= OnUserJoined;
			this.client.Users.UserDisconnected -= OnUserDisconnected;
			this.client.Users.UserChangedChannel -= OnUserChangedChannel;
		}

		private void OnUserDisconnected (object sender, UserEventArgs e)
		{
			if (!e.User.Equals (client.CurrentChannel))
				Speak (e.User.Nickname + " has left the server.");
		}

		private void OnUserJoined (object sender, UserEventArgs e)
		{
			if (!e.User.Equals (client.CurrentUser))
				Speak (e.User.Nickname + " has joined the server.");
		}

		void OnUserChangedChannel (object sender, ChannelChangedEventArgs e)
		{
			if (e.User.Equals (client.CurrentUser))
			    return;

			if (e.TargetChannel.Equals (client.CurrentChannel))
				Speak (e.User.Nickname + " joined the channel.");
			else if (e.PreviousChannel.Equals (client.CurrentChannel))
				Speak (e.User.Nickname + " left the channel.");
		}

		private readonly MediaPlayerIntegration media;
		private readonly GablarskiClient client;
		private static readonly SpeechSynthesizer speech = new SpeechSynthesizer ();

		private void Speak (string say)
		{
			ThreadPool.QueueUserWorkItem ((o) =>
			{
				media.AddTalker ();
				speech.Speak ((string)o);
				media.RemoveTalker ();
			}, say);
		}
	}
}