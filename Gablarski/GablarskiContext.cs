using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;

namespace Gablarski
{
	public class GablarskiContext
	{
		public void AddUser (UserInfo user)
		{
			lock (userLock)
			{
				if (this.users == null)
					this.users = new Dictionary<object, UserInfo>();

				this.users.Add (user.UserId, user);
			}
		}

		public void RemoveUser (UserInfo user)
		{
			
		}

		private readonly object channelLock = new object();
		private Dictionary<object, Channel> channels;

		private readonly object sourceLock = new object();
		private Dictionary<int, MediaSourceBase> sources;

		private readonly object userLock = new object();
		private Dictionary<object, UserInfo> users;
	}
}