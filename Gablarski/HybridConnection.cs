using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Gablarski
{
	public class HybridConnection
	{
		public HybridConnection (NetworkStream tcp, IPEndPoint endPoint)
		{
			
		}

		private NetworkStream tcp;

		private static UdpClient udp = new UdpClient ();
	}
}