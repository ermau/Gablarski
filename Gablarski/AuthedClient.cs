using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Mono.Rocks;

namespace Gablarski
{
	public class AuthedClient
	{
		internal AuthedClient (int authHash, bool isLittleEndian)
		{
			this.AuthHash = authHash;
			this.IsLittleEndian = isLittleEndian;
		}

		public int AuthHash
		{
			get;
			internal set;
		}

		public bool IsLittleEndian
		{
			get;
			internal set;
		}
	}
}