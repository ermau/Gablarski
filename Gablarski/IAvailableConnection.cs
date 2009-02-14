using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IAvailableConnection
	{
		event EventHandler<ConnectionMadeEventArgs> ConnectionMade;

		void StartListening ();
		void StopListening ();
	}

	public class ConnectionMadeEventArgs
		: EventArgs
	{
		public ConnectionMadeEventArgs (IConnection connection)
		{
			this.Connection = connection;
		}

		public IConnection Connection
		{
			get;
			private set;
		}
	}
}