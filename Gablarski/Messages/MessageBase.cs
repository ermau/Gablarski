using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public abstract class MessageBase
	{
		protected MessageBase (IEndPoint endpoint)
		{
			this.EndPoints = new[] { endpoint };
		}

		protected MessageBase (IEndPoint endpoint, IValueReader payload)
		{
			this.EndPoints = new[] { endpoint };
			this.ReadPayload (payload);
		}

		protected MessageBase (IEnumerable<IEndPoint> endpoints)
		{
			this.EndPoints = endpoints;
		}

		public IEnumerable<IEndPoint> EndPoints
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

		protected virtual bool Reliable
		{
			get { return true; }
		}

		protected abstract void WritePayload (IValueWriter writer);
		protected abstract void ReadPayload (IValueReader reader);
	}
}