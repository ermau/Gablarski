using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public abstract class Message<TMessage>
		: MessageBase
	{
		protected Message (TMessage messageType, AuthedClient client)
			: base (client)
		{
			this.MessageType = messageType;
		}

		protected Message (TMessage messageType, IEnumerable<AuthedClient> clients)
			: base (clients)
		{
			this.MessageType = messageType;
		}

		public TMessage MessageType
		{
			get;
			protected set;
		}
	}
}