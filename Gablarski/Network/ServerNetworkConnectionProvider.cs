using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Network
{
	public class ServerNetworkConnectionProvider
		: IConnectionProvider
	{
		#region IConnectionProvider Members

		public event EventHandler<ConnectionMadeEventArgs> ConnectionMade;

		public void StartListening ()
		{
			throw new NotImplementedException ();
		}

		public void StopListening ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		protected virtual void OnConnectionMade (ConnectionMadeEventArgs e)
		{
			var connection = this.ConnectionMade;
			if (connection != null)
				connection (this, e);
		}
	}
}