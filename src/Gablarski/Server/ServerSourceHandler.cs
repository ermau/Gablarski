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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
			if (context == null)
				throw new ArgumentNullException ("context");
			if (manager == null)
				throw new ArgumentNullException ("manager");

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
			get { return manager[key]; }
		}

		IEnumerable<AudioSource> IServerSourceHandler.this [UserInfo user]
		{
			get { return manager[user]; }
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
			if (!e.Connection.IsConnected)
				return;

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
			if (!e.Connection.IsConnected)
				return;
			
			e.Connection.Send (new SourceListMessage (manager));
		}

		internal void ClientAudioSourceStateChangeMessage (MessageReceivedEventArgs e)
		{
			var msg = (ClientAudioSourceStateChangeMessage)e.Message;

			UserInfo speaker;
			if (!CanSendFromSource (e.Connection, msg.SourceId, out speaker))
				return;

			context.Users.Send (new AudioSourceStateChangeMessage { Starting = msg.Starting, SourceId = msg.SourceId },
			                    (con, user) => con != e.Connection);
		}

		internal void SendAudioDataMessage (MessageReceivedEventArgs e)
		{
			var msg = (SendAudioDataMessage)e.Message;

			UserInfo speaker;
			if (!CanSendFromSource (e.Connection, msg.SourceId, out speaker))
				return;

			if (!context.GetPermission (PermissionName.SendAudio, speaker))
			{
				e.Connection.Send (new PermissionDeniedMessage (ClientMessageType.AudioData));
				return;
			}

			if (msg.TargetIds.Length > 1 && !context.GetPermission (PermissionName.SendAudioToMultipleTargets, speaker))
			{
				e.Connection.Send (new PermissionDeniedMessage (ClientMessageType.AudioData));
				return;
			}

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

		private bool CanSendFromSource (IConnection connection, int sourceId, out UserInfo speaker)
		{
			speaker = null;

			if (connection == null || !connection.IsConnected)
				return false;

			var source = manager[sourceId];
			if (source == null || source.IsMuted)
				return false;

			speaker = context.UserManager.GetUser (connection);

			return (speaker != null && !speaker.IsMuted && speaker.UserId == source.OwnerId);
		}
	}
}