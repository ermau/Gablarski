using System;
using System.Collections.Generic;
using System.Text;

namespace Lidgren.Network
{
	public sealed partial class NetConnection
	{
		public void Approve()
		{
			if (m_approved == true)
				throw new NetException("Connection is already approved!");

			//
			// Continue connection phase
			//

			// Add connection
			m_approved = true;

			NetServer server = m_owner as NetServer;
			server.AddConnection(NetTime.Now, this);
		}

		public void Disapprove(string reason)
		{
			if (m_approved == true)
				throw new NetException("Connection is already approved!");

			// send connectionrejected
			NetBuffer buf = new NetBuffer();
			buf.Write(reason);
			m_owner.QueueSingleUnreliableSystemMessage(
				NetSystemType.ConnectionRejected,
				buf,
				m_remoteEndPoint,
				false
			);

			m_requestDisconnect = true;
			m_requestLinger = 0.0f;
			m_requestSendGoodbye = !string.IsNullOrEmpty(reason);
			m_futureDisconnectReason = reason;
		}
	}
}
