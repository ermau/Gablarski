using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Gablarski
{
	public class UserConnection
	{
		public UserConnection (NetBase net, NetConnection connection)
		{
			this.net = net;
			this.Connection = connection;
		}

		public User User
		{
			get;
			set;
		}

		public int AuthHash
		{
			get;
			internal set;
		}

		public NetConnection Connection
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