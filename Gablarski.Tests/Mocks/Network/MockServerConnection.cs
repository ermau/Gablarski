using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using System.Threading;
using NUnit.Framework;

namespace Gablarski.Tests
{
	public class MockServerConnection
		: MockConnectionBase
	{
		public MockClientConnection Client
		{
			get
			{
				if (this.client == null)
				{
					this.client = new MockClientConnection (this);
				}

				return this.client;
			}
		}

		public override void Send (MessageBase message)
		{
			if (this.client == null)
			{
				this.client = new MockClientConnection (this);
			}

			this.client.Receive (message);
		}
		
		private MockClientConnection client;

		protected override string Name
		{
			get { return "Server"; }
		}
	}
}
