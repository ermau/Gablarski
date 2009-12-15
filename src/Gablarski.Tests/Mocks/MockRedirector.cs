using System;
using Gablarski.Server;
using System.Net;

namespace Gablarski.Tests
{
	public class MockRedirector
		: IRedirector
	{
		private readonly string checkFor;
		private readonly IPEndPoint sendTo;
		
		public MockRedirector (string checkFor, IPEndPoint sendTo)
		{
			this.checkFor = checkFor;
			this.sendTo = sendTo;
		}
		
		public IPEndPoint CheckRedirect (string host, int port)
		{
			return (host == checkFor) ? sendTo : null;
		}
	}
}