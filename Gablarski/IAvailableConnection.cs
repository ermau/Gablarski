using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IAvailableConnection
		: IConnection
	{
		void StartListening ();
		void StopListening ();
	}

	public class MessageReceivedEventArgs
		: EventArgs
	{
		public MessageReceivedEventArgs (MessageBase message)
		{
			this.Message = message;
		}

		/// <summary>
		/// Gets the message that was received.
		/// </summary>
		public MessageBase Message
		{
			get;
			private set;
		}
	}
}