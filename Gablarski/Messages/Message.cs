using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public abstract class Message<TMessage>
		: MessageBase
	{
		protected Message (TMessage messageType, IEndPoint endpoint)
			: base (endpoint)
		{
			this.MessageType = messageType;
		}

		protected Message (TMessage messageType, IEndPoint endpoint, IValueReader payload)
			: base (endpoint, payload)
		{
			this.MessageType = messageType;
		}

		protected Message (TMessage messageType, IEnumerable<IEndPoint> endpoints)
			: base (endpoints)
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