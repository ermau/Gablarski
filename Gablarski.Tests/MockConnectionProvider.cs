using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;

namespace Gablarski.Tests
{
	public class MockConnectionProvider
		: IConnectionProvider
	{
		public IConnection EstablishConnection ()
		{
			var connection = new MockServerConnection ();

			var connectionMade = this.ConnectionMade;
			if (connectionMade != null)
				connectionMade (this, new ConnectionEventArgs (connection));

			return connection;
		}

		#region IConnectionProvider Members

		public event EventHandler<ConnectionEventArgs> ConnectionMade;

		public void StartListening ()
		{
			this.listening = true;
		}

		public void StopListening ()
		{
			this.listening = false;
		}

		#endregion

		private bool listening = false;
	}
}