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
		internal ClientUser (IClientConnection client)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
		}

		public ClientUser (UserInfo user, IClientConnection client)
			: base (user.Nickname, user.Username, user.UserId, user.CurrentChannelId, user.Muted)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
		}

		public ClientUser (string nickname, int userId, int currentChannelId, IClientConnection client, bool muted)
			: base (nickname, null, userId, currentChannelId, muted)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
		}

		public bool IsIgnored
		{
			get; private set;
		}

		/// <summary>
		/// Moves this user to <paramref name="targetChannel"/>.
		/// </summary>
		/// <param name="targetChannel">The channel to move this user to.</param>
		public void Move (ChannelInfo targetChannel)
		{
			if (targetChannel == null)
				throw new ArgumentNullException ("targetChannel");

			this.client.Send (new ChannelChangeMessage (this.UserId, targetChannel.ChannelId));
		}

		public void ToggleIgnore()
		{
			this.IsIgnored = !this.IsIgnored;
		}

		public void ToggleMute ()
		{
			this.client.Send (new RequestMuteMessage { Target = this.Username, Type = MuteType.User, Unmute = !this.Muted });
		}

		private readonly IClientConnection client;
	}
}