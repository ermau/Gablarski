using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Rocks;

namespace Gablarski.Messages
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

		public T GetMessage<T> ()
			where T : MessageBase
		{
			return (T) this;
		}

		protected abstract ushort MessageTypeCode
		{
			get;
		}

		protected abstract bool SendAuthHash
		{
			get;
		}

		protected abstract void WritePayload (IValueWriter writer);
	}
}