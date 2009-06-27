using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using System.Threading;
using System.Net;
using NUnit.Framework;

namespace Gablarski.Tests
{
	public class MockClientConnection
		: MockConnectionBase, IClientConnection
	{
		public MockClientConnection (MockServerConnection server)
		{
			this.server = server;
		}

		#region IClientConnection Members

		public event EventHandler Connected;

		public void Connect (string host, int port)
		{
			throw new NotSupportedException ();
		}

		public void Connect (IPEndPoint endpoint)
		{
			throw new NotSupportedException ();
		}
		#endregion

		public override void Send (MessageBase message)
		{
			this.server.Receive (message);
		}

		private readonly MockServerConnection server;

		protected override string Name
		{
			get { return "Client"; }
		}
	}
}