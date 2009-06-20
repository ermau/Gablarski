using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientUser
		: UserInfo
	{
		internal ClientUser ()
		{
		}

		public ClientUser (UserInfo user, IClientConnection client)
			: base (user.Nickname, user.UserId, user.CurrentChannelId)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
		}

		public ClientUser (string nickname, object userId, object currentChannelId, IClientConnection client)
			: base (nickname, userId, currentChannelId)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
		}

		/// <summary>
		/// Moves this user to <paramref name="targetChannel"/>.
		/// </summary>
		/// <param name="targetChannel">The channel to move this user to.</param>
		public void Move (Channel targetChannel)
		{
			if (targetChannel == null)
				throw new ArgumentNullException ("targetChannel");

			this.client.Send (new ChangeChannelMessage (this.UserId, targetChannel.ChannelId));
		}

		private readonly IClientConnection client;
	}
}