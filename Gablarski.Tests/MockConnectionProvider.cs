using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;
using Gablarski.Messages;

namespace Gablarski.Tests
{
	public class MockConnectionProvider
		: IConnectionProvider
	{
		public MockServerConnection EstablishConnection ()
		{
			var connection = new MockServerConnection ();

			var connectionMade = this.ConnectionMade;
			if (connectionMade != null)
				connectionMade (this, new ConnectionEventArgs (connection));

			return connection;
		}

		public bool IsListening
		{
			get;
			private set;
		}

		#region IConnectionProvider Members

		public event EventHandler<ConnectionEventArgs> ConnectionMade;

		public void StartListening ()
		{
			this.IsListening = true;
		}

		public void StopListening ()
		{
			this.IsListening = false;
		}

		#endregion
	}
}