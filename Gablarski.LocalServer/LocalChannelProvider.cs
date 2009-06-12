using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;

namespace Gablarski.LocalServer
{
	public class LocalChannelProvider
		//: IChannelProvider
	{
		public event EventHandler ChannelsUpdatedExternally;

		public bool UpdateSupported
		{
			get { throw new NotImplementedException (); }
		}

		public Channel DefaultChannel
		{
			get { throw new NotImplementedException (); }
		}

		public IEnumerable<Channel> GetChannels ()
		{
			throw new NotImplementedException ();
		}

		public void SaveChannel (Channel channel)
		{
			throw new NotImplementedException ();
		}

		public void DeleteChannel (Channel channel)
		{
			throw new NotImplementedException ();
		}
	}
}