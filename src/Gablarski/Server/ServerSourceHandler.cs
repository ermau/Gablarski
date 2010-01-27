using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Messages;

namespace Gablarski.Server
{
	public class ServerSourceHandler
		: IServerSourceHandler
	{
		private readonly IServerContext context;
		private readonly IServerSourceManager manager;

		public ServerSourceHandler (IServerContext context, IServerSourceManager manager)
		{
			this.context = context;
			this.manager = manager;
		}

		public IEnumerator<AudioSource> GetEnumerator()
		{
			return Enumerable.Empty<AudioSource>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public AudioSource this [int key]
		{
			get { return null; }
		}

		IEnumerable<AudioSource> IServerSourceHandler.this [UserInfo user]
		{
			get { throw new NotImplementedException(); }
		}

		public void Remove (AudioSource source)
		{
			context.Users.Send (new SourcesRemovedMessage (new[] { source }));

			manager.Remove (source);
		}

		public void Remove (UserInfo user)
		{
			IEnumerable<AudioSource> sources = manager[user];
			if (sources == null)
				return;

			context.Users.Send (new SourcesRemovedMessage (sources));
			manager.Remove (user);
		}

		internal void RequestSourceMessage (MessageReceivedEventArgs e)
		{
			var request = (RequestSourceMessage)e.Message;

			SourceResult result = SourceResult.FailedUnknown;

			UserInfo requestingUser = context.UserManager.GetUser (e.Connection);
			if (requestingUser == null)
				return;

			if (!context.GetPermission (PermissionName.RequestSource, requestingUser))
				result = SourceResult.FailedPermissions;

			if (String.IsNullOrEmpty (request.Name) 
				|| AudioCodecArgs.IsInvalidBitrate (request.AudioSettings.Bitrate)
				|| AudioCodecArgs.IsInvalidChannels (request.AudioSettings.Channels)
				|| AudioCodecArgs.IsInvalidComplexity (request.AudioSettings.Complexity)
				|| AudioCodecArgs.IsInvalidFrequency (request.AudioSettings.Frequency)
				|| AudioCodecArgs.IsInvalidFrameSize (request.AudioSettings.FrameSize))
			{
				result = SourceResult.FailedInvalidArguments;
			}

			AudioSource source = null;
			try
			{
				if (result == SourceResult.FailedUnknown)
				{
					int bitrate = context.Settings.DefaultAudioBitrate;
					if (request.AudioSettings.Bitrate != 0)
						bitrate = request.AudioSettings.Bitrate.Trim (context.Settings.MinimumAudioBitrate, context.Settings.MaximumAudioBitrate);

					if (manager.IsSourceNameTaken (requestingUser, request.Name))
						result = SourceResult.FailedDuplicateSourceName;
					else
					{
						source = manager.Create (request.Name, requestingUser,
						                         new AudioCodecArgs (request.AudioSettings) { Bitrate = bitrate });
						result = SourceResult.Succeeded;
					}
				}
			}
			catch (OverflowException)
			{
				result = SourceResult.FailedLimit;
			}
			finally
			{
				e.Connection.Send (new SourceResultMessage (request.Name, result, source));
				if (result == SourceResult.Succeeded)
				{
					context.Connections.Send (new SourceResultMessage (request.Name, SourceResult.NewSource, source),
					                    c => c != e.Connection);
				}
			}
		}

		internal void RequestMuteSourceMessage (MessageReceivedEventArgs e)
		{
			var request = (RequestMuteMessage)e.Message;

			if (!context.GetPermission (PermissionName.MuteAudioSource, e.Connection))
			{
				e.Connection.Send (new PermissionDeniedMessage { DeniedMessage = ClientMessageType.RequestMuteSource });
				return;
			}

			var source = manager[request.TargetId];
			if (source == null)
				return;

			bool muted = manager.ToggleMute (source);
			context.Connections.Send (new MutedMessage { Target = source.Id, Type = MuteType.AudioSource, Unmuted = !muted });
		}

		internal void RequestSourceListMessage (MessageReceivedEventArgs e)
		{
			e.Connection.Send (new SourceListMessage (manager));
		}

		internal void ClientAudioSourceStateChangeMessage (MessageReceivedEventArgs e)
		{
			var msg = (ClientAudioSourceStateChangeMessage)e.Message;

			var speaker = context.UserManager.GetUser (e.Connection);

			if (speaker.IsMuted)
				return;

			context.Users.Send (new AudioSourceStateChangeMessage { Starting = msg.Starting, SourceId = msg.SourceId },
			                    (con, user) => con != e.Connection);
		}

		internal void SendAudioDataMessage (MessageReceivedEventArgs e)
		{
			var msg = (SendAudioDataMessage)e.Message;

			UserInfo speaker = context.UserManager.GetUser (e.Connection);

			if (speaker.IsMuted)
				return;

			if (!context.GetPermission (PermissionName.SendAudio, speaker))
				return;

			if (msg.TargetIds.Length > 1 && !context.GetPermission (PermissionName.SendAudioToMultipleTargets, speaker))
				return;

			var sendMessage = new AudioDataReceivedMessage { Data = msg.Data, SourceId = msg.SourceId };

			if (msg.TargetType == TargetType.Channel)
			{
				for (int i = 0; i < msg.TargetIds.Length; ++i)
					context.Users.Send (sendMessage, (con, user) => con != e.Connection && user.CurrentChannelId == msg.TargetIds[i]);
			}
			else if (msg.TargetType == TargetType.User)
			{
				context.Users.Send (sendMessage, (con, user) => con != e.Connection && msg.TargetIds.Contains (user.UserId));
			}
		}
	}
}