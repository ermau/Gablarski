using System;
using System.Linq;
using Gablarski.Messages;
using Tempest;
using Tempest.Social;

namespace Gablarski.Server
{
	public class GablarskiSocialServer
		: SocialServer
	{
		public GablarskiSocialServer (IWatchListProvider provider, IIdentityProvider identityProvider)
			: base (provider, identityProvider)
		{
			this.RegisterMessageHandler<JoinVoiceMessage> (OnJoinVoiceMessage);
		}

		private async void OnJoinVoiceMessage (MessageEventArgs<JoinVoiceMessage> e)
		{
			var person = await GetPersonAsync (e.Connection);
			if (person == null) {
				await e.Connection.DisconnectAsync();
				return;
			}

			Group group;
			lock (SyncRoot) {
				if (!Groups.TryGetGroup (e.Message.GroupId, out group) || !group.Participants.Contains (person.Identity))
					return;
			}

			IConnection connection;
			lock (SyncRoot) {
				connection = GetConnection (group.OwnerId);
			}

			if (connection == null)
				return;

			var join = new JoinVoiceMessage {
				GroupId = e.Message.GroupId,
				Target = e.Message.Target
			};

			try {
				JoinVoiceMessage response = await connection.SendFor<JoinVoiceMessage> (join, responseTimeout: 30000);
				await e.Connection.SendResponseAsync (e.Message, response);
			} catch (OperationCanceledException) {
			}
		}
	}
}