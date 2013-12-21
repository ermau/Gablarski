// Copyright (c) 2011, Eric Maupin
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
using Tempest;

namespace Gablarski.Server
{
	public class ServerSourceHandler
		: IServerSourceHandler
	{
		private readonly IGablarskiServerContext context;
		private readonly IServerSourceManager manager;

		public ServerSourceHandler (IGablarskiServerContext context, IServerSourceManager manager)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			this.context = context;
			this.manager = manager;

			this.context.RegisterMessageHandler<RequestSourceMessage> (RequestSourceMessage);
			this.context.RegisterMessageHandler<ClientAudioDataMessage> (OnClientAudioDataMessage);
			this.context.RegisterMessageHandler<ClientAudioSourceStateChangeMessage> (ClientAudioSourceStateChangeMessage);
			this.context.RegisterMessageHandler<RequestMuteSourceMessage> (RequestMuteSourceMessage);
			this.context.RegisterMessageHandler<RequestSourceListMessage> (RequestSourceListMessage);
		}

		public IEnumerator<AudioSource> GetEnumerator()
		{
			return manager.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public AudioSource this [int key]
		{
			get { return manager[key]; }
		}

		public IEnumerable<AudioSource> this [IUserInfo user]
		{
			get { return manager[user]; }
		}

		public void Remove (AudioSource source)
		{
			foreach (var connection in context.Connections)
				connection.SendAsync (new SourcesRemovedMessage (new[] { source }));

			manager.Remove (source);
		}

		public void Remove (IUserInfo user)
		{
			IEnumerable<AudioSource> sources = manager[user];
			if (sources == null)
				return;

			foreach (var connection in context.Connections)
				connection.SendAsync (new SourcesRemovedMessage (sources));

			manager.Remove (user);
		}

		internal void RequestSourceMessage (MessageEventArgs<RequestSourceMessage> e)
		{
			var request = (RequestSourceMessage)e.Message;

			SourceResult result = SourceResult.FailedUnknown;

			IUserInfo requestingUser = context.Users[e.Connection];
			if (requestingUser == null)
				return;

			if (!context.GetPermission (PermissionName.RequestSource, requestingUser))
				result = SourceResult.FailedPermissions;

			if (String.IsNullOrEmpty (request.Name) 
				|| AudioCodecArgs.IsInvalidBitrate (request.AudioSettings.Bitrate)
				|| AudioCodecArgs.IsInvalidComplexity (request.AudioSettings.Complexity)
				|| AudioCodecArgs.IsInvalidFrequency (request.AudioSettings.SampleRate)
				|| AudioCodecArgs.IsInvalidFrameSize (request.AudioSettings.SampleRate, request.AudioSettings.Channels, request.AudioSettings.FrameSize))
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
				e.Connection.SendAsync (new SourceResultMessage (request.Name, result, source));
				if (result == SourceResult.Succeeded)
				{
					foreach (IConnection connection in context.Connections)
					{
						if (connection == e.Connection)
							continue;

						connection.SendAsync (new SourceResultMessage (request.Name, SourceResult.NewSource, source));
					}
				}
			}
		}

		internal void RequestMuteSourceMessage (MessageEventArgs<RequestMuteSourceMessage> e)
		{
			var request = (RequestMuteMessage)e.Message;
			if (!e.Connection.IsConnected)
				return;

			if (!context.GetPermission (PermissionName.MuteAudioSource, e.Connection))
			{
				e.Connection.SendAsync (new PermissionDeniedMessage { DeniedMessage = GablarskiMessageType.RequestMuteSource });
				return;
			}

			var source = manager[request.TargetId];
			if (source == null)
				return;

			bool muted = manager.ToggleMute (source);
			foreach (IConnection connection in this.context.Connections)
				connection.SendAsync (new SourceMutedMessage { SourceId = source.Id, Unmuted = !muted });
		}

		internal void RequestSourceListMessage (MessageEventArgs<RequestSourceListMessage> e)
		{
			if (!e.Connection.IsConnected)
				return;
			
			e.Connection.SendAsync (new SourceListMessage (manager));
		}

		internal void ClientAudioSourceStateChangeMessage (MessageEventArgs<ClientAudioSourceStateChangeMessage> e)
		{
			var msg = e.Message;

			IUserInfo speaker;
			if (!CanSendFromSource (e.Connection, msg.SourceId, out speaker))
				return;

			foreach (IConnection connection in context.Users.Connections)
			{
				if (connection == e.Connection)
					continue;

				connection.SendAsync (new AudioSourceStateChangeMessage { Starting = msg.Starting, SourceId = msg.SourceId });
			}
		}

		internal void OnClientAudioDataMessage (MessageEventArgs<ClientAudioDataMessage> e)
		{
			var msg = e.Message;

			IUserInfo speaker;
			if (!CanSendFromSource (e.Connection, msg.SourceId, out speaker))
				return;

			if (!context.GetPermission (PermissionName.SendAudio, speaker))
			{
				e.Connection.SendAsync (new PermissionDeniedMessage (GablarskiMessageType.ClientAudioData));
				return;
			}

			if (msg.TargetIds.Length > 1 && !context.GetPermission (PermissionName.SendAudioToMultipleTargets, speaker))
			{
				e.Connection.SendAsync (new PermissionDeniedMessage (GablarskiMessageType.ClientAudioData));
				return;
			}

			if (msg.TargetType == TargetType.Channel)
			{
				for (int i = 0; i < msg.TargetIds.Length; ++i)
				{
					foreach (var user in this.context.Users.Where (u => u.CurrentChannelId == msg.TargetIds[i]))
					{
						IConnection connection = this.context.Users[user];
						if (connection == null || connection == e.Connection)
							continue;

						connection.SendAsync (new ServerAudioDataMessage {
							Data = msg.Data,
							Sequence = msg.Sequence,
							SourceId = msg.SourceId
						});
					}
				}
			}
			else if (msg.TargetType == TargetType.User)
			{
				for (int i = 0; i < msg.TargetIds.Length; ++i)
				{
					IUserInfo user = this.context.Users[msg.TargetIds[i]];
					if (user == null)
						continue;

					IConnection connection = this.context.Users[user];
					if (connection == null || connection == e.Connection)
						continue;

					connection.SendAsync (new ServerAudioDataMessage {
						Data = msg.Data,
						Sequence = msg.Sequence,
						SourceId = msg.SourceId
					});
				}
			}
		}

		private bool CanSendFromSource (IConnection connection, int sourceId, out IUserInfo speaker)
		{
			speaker = null;

			if (connection == null || !connection.IsConnected)
				return false;

			var source = manager[sourceId];
			if (source == null || source.IsMuted)
				return false;

			speaker = context.Users[connection];

			return (speaker != null && !speaker.IsMuted && speaker.UserId == source.OwnerId);
		}
	}
}