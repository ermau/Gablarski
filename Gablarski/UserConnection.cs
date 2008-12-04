using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Gablarski
{
	public class UserConnection
	{
		public UserConnection (int hash, NetBase net)
		{
			this.AuthHash = hash;
			this.net = net;
		}

		public User User
		{
			get;
			set;
		}

		public int AuthHash
		{
			get;
			private set;
		}

		public NetBuffer CreateBuffer()
		{
			return net.CreateBuffer ();
		}

		private readonly NetBase net;
	}
}