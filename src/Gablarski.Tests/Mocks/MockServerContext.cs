using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;

namespace Gablarski.Tests.Mocks
{
	public class MockServerContext
		: IServerContext
	{
		public object SyncRoot
		{
			get { return syncRoot; }
		}

		public IBackendProvider BackendProvider
		{
			get;
			set;
		}

		public IUserProvider UserProvider
		{
			get;
			set;
		}

		public IPermissionsProvider PermissionsProvider
		{
			get;
			set;
		}

		public IChannelProvider ChannelsProvider
		{
			get;
			set;
		}

		public int ProtocolVersion
		{
			get { return GablarskiServer.ProtocolVersion; }
		}

		public IConnectionHandler Connections
		{
			get { return Users; }
		}

		public IServerUserHandler Users
		{
			get;
			set;
		}

		public IServerUserManager UserManager
		{
			get;
			set;
		}

		public IServerSourceHandler Sources
		{
			get;
			set;
		}

		public IServerChannelHandler Channels
		{
			get;
			set;
		}

		public IEnumerable<IRedirector> Redirectors
		{
			get;
			set;
		}

		public ServerSettings Settings
		{
			get;
			set;
		}

		private readonly object syncRoot = new object();
	}
}