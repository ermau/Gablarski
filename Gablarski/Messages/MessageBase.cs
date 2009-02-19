using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public abstract class MessageBase
	{
		#region Constructors
		protected MessageBase ()
		{
		}

		protected MessageBase (IValueReader payload)
		{
			this.ReadPayload (payload);
		}
		#endregion

		#region Internals
		protected abstract ushort MessageTypeCode
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