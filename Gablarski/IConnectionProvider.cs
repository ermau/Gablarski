using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IConnectionProvider
	{
		event EventHandler<ConnectionEventArgs> ConnectionMade;

		void StartListening ();
		void StopListening ();
	}

	public class ConnectionEventArgs
		: EventArgs
	{
		public ConnectionEventArgs (IConnection connection)
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