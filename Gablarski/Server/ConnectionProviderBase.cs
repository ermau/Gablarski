using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gablarski.Server
{
	public abstract class ConnectionProviderBase
		: IConnectionProvider
	{
		#region IConnectionProvider Members

		public event EventHandler<ConnectionEventArgs> ConnectionMade;

		public event EventHandler<MessageReceivedEventArgs> ConnectionlessMessageReceived;

		public IdentifyingTypes IdentifyingTypes
		{
			get; set;
		}

		public void StartListening()
		{
			this.listening = true;
			this.Start();
			(this.listenerThread = new Thread (this.ListenChecker)
			{
				Name = this.GetType().Name + " Listen checker thread",
				IsBackground = true
			}).Start();
		}

		public void StopListening()
		{
			this.Stop();

			this.listening = false;

			if (this.listenerThread != null)
			{
				this.listenerThread.Join();
				this.listenerThread = null;
			}
		}

		#endregion

		protected volatile bool listening;
		protected Thread listenerThread;

		protected abstract void Start();
		protected abstract void Stop();

		/// <summary>
		/// Checks for a connection, can block, should return null if non-blocking and not found.
		/// </summary>
		/// <returns><c>IConnection</c> if connection found, <c>null</c> otherwise</returns>
		protected abstract IConnection CheckForConnection();

		protected void ListenChecker()
		{
			while (this.listening)
			{
				IConnection connection = CheckForConnection();
				if (connection != null)
					this.OnConnectionMade (new ConnectionEventArgs (connection));

				Thread.Sleep (1);
			}
		}

		protected virtual void OnConnectionMade (ConnectionEventArgs e)
		{
			var connection = this.ConnectionMade;
			if (connection != null)
				connection (this, e);
		}
	}
}