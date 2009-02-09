using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IConnection
	{
		event EventHandler<MessageReceivedEventArgs> MessageReceived;

		void StartListening ();
		void StopListening ();

		void Send (MessageBase message);
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