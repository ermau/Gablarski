using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public abstract class Message<TMessage>
	{
		protected Message (TMessage messageType)
		{
			this.MessageType = messageType;
		}

		protected Message (TMessage messageType, AuthedClient client)
			: this (messageType)
		{
			this.Connections = new[] { client };
		}

		protected Message (TMessage messageType, IEnumerable<AuthedClient> clients)
			: this (messageType)
		{
			this.Connections = clients;
		}

		public TMessage MessageType
		{
			get;
			private set;
		}

		public IEnumerable<AuthedClient> Connections
		{
			get;
			private set;
		}

		protected abstract uint MessageTypeCode
		{
			get;
		}

		protected abstract bool SendAuthHash
		{
			get;
		}
	}
}