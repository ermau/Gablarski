using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using Gablarski.Media.Sources;

namespace Gablarski.Server
{
	public partial class GablarskiServer
	{
		protected GablarskiServer ()
		{
			this.Handlers = new Dictionary<ClientMessageType, Action<MessageReceivedEventArgs>>
			{
				{ ClientMessageType.Connect, ClientConnected },
				{ ClientMessageType.Disconnect, ClientDisconnected },
				{ ClientMessageType.Login, UserLoginAttempt },
				{ ClientMessageType.RequestSource, ClientRequestsSource },
				{ ClientMessageType.AudioData, AudioDataReceived },
			};
		}

		private readonly Dictionary<ClientMessageType, Action<MessageReceivedEventArgs>> Handlers;

		private void ClientConnected (MessageReceivedEventArgs e)
		{
			var msg = (ConnectMessage)e.Message;

			if (msg.ApiVersion < MinimumAPIVersion)
			{
				e.Connection.Send (new ConnectionRejectedMessage (ConnectionRejectedReason.IncompatibleVersion));
				e.Connection.Disconnect ();
				return;
			}

			e.Connection.Send (new ServerInfoMessage (this.serverInfo));
			e.Connection.Send (new PlayerListMessage (this.connections.Players));
			e.Connection.Send (new SourceListMessage (this.GetSourceInfoList()));
		}

		protected void ClientRequestsSource (MessageReceivedEventArgs e)
		{
			var request = (RequestSourceMessage)e.Message;

			SourceResult result = SourceResult.FailedUnknown;
			int sourceId = -1;

			long playerId = this.connections.GetPlayerId (e.Connection);
			if (playerId == 0 || !this.permissionProvider.GetPermissions (playerId).CanRequestSource())
				result = SourceResult.FailedPermissions;

			IMediaSource source = null;
			try
			{
				int index = 0;
				lock (sourceLock)
				{
					if (!sources.ContainsKey (e.Connection))
						sources.Add (e.Connection, new List<IMediaSource> ());

					if (!sources[e.Connection].Any (s => s != null && s.GetType () == request.MediaSourceType))
					{
						sourceId = sources.Sum (kvp => kvp.Value.Count);
						index = sources[e.Connection].Count;
						sources[e.Connection].Add (null);
					}
					else
						result = SourceResult.FailedPermittedSingleSourceOfType;
				}

				if (result == SourceResult.FailedUnknown)
				{
					source = MediaSources.Create (request.MediaSourceType, sourceId);
					if (source != null)
					{
						lock (sourceLock)
						{
							sources[e.Connection][index] = source;
						}

						result = SourceResult.Succeeded;
					}
					else
						result = SourceResult.FailedNotSupportedType;
				}
			}
			catch (OverflowException)
			{
				result = SourceResult.FailedLimit;
			}
			finally
			{
				MediaSourceInfo sourceInfo = new MediaSourceInfo
				{
					SourceId = sourceId,
					PlayerId = playerId,
					MediaType = (source != null) ? source.Type : MediaType.None,
					SourceTypeName = request.MediaSourceType.AssemblyQualifiedName
				};

				e.Connection.Send (new SourceResultMessage (result, sourceInfo));
				if (result == SourceResult.Succeeded)
				{
					connections.Send (new SourceResultMessage (SourceResult.NewSource, sourceInfo), (IConnection c) => c != e.Connection);
				}
			}
		}
	}
}