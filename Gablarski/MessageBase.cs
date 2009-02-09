using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public abstract class MessageBase
	{
		protected MessageBase (AuthedClient client)
		{
			this.Clients = new[] { client };
		}

		protected MessageBase (IEnumerable<AuthedClient> clients)
		{
			this.Clients = clients;
		}

		public IEnumerable<AuthedClient> Clients
		{
			get;
			private set;
		}

		protected abstract ushort MessageTypeCode
		{
			get;
		}

		protected abstract bool SendAuthHash
		{
			get;
		}

		protected abstract byte[] GetPayload ();
	}
}