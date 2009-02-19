using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public abstract class MessageBase
	{
		#region Constructors
		protected MessageBase (IConnection recipient)
		{
			this.Recipients = new[] { recipient };
		}

		protected MessageBase (IConnection recipient, IValueReader payload)
		{
			this.Recipients = new[] { recipient };
			this.ReadPayload (payload);
		}

		protected MessageBase (IEnumerable<IConnection> recipients)
		{
			this.Recipients = recipients;
		}
		#endregion

		#region Public Properties
		public IEnumerable<IConnection> Recipients
		{
			get;
			private set;
		}
		#endregion

		#region Internals
		protected abstract ushort MessageTypeCode
		{
			get;
		}

		protected abstract bool SendAuthHash
		{
			get;
		}

		protected virtual bool Reliable
		{
			get { return true; }
		}

		protected abstract void WritePayload (IValueWriter writer);
		protected abstract void ReadPayload (IValueReader reader);
		#endregion
	}
}