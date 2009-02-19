using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public abstract class Message<TMessage>
		: MessageBase
	{
		protected Message (TMessage messageType, IConnection recipient)
			: base (recipient)
		{
			this.MessageType = messageType;
		}

		protected Message (TMessage messageType, IConnection recipient, IValueReader payload)
			: base (recipient, payload)
		{
			this.MessageType = messageType;
		}

		protected Message (TMessage messageType, IEnumerable<IConnection> recipients)
			: base (recipients)
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