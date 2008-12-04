using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Lidgren.Network
{
	//
	// Various EventArgs derivatives
	//

	public sealed class NetConnectionRequestEventArgs : EventArgs
	{
		private IPEndPoint m_endPoint;
		private byte[] m_hailData;
		private string m_denialReason;
		private bool m_mayConnect;

		public IPEndPoint Endpoint { get { return m_endPoint; } set { m_endPoint = value; } }
		public byte[] HailData { get { return m_hailData; } set { m_hailData = value; } }
		public string DenialReason { get { return m_denialReason; } set { m_denialReason = value; } }
		public bool MayConnect { get { return m_mayConnect; } set { m_mayConnect = value; } }

		public NetConnectionRequestEventArgs(IPEndPoint endpoint, byte[] hailData)
		{
			m_endPoint = endpoint;
			m_hailData = hailData;
			m_mayConnect = true;
			m_denialReason = null;
		}
	}

	public sealed class NetDiscoveryRequestEventArgs : EventArgs
	{
		private IPEndPoint m_endPoint;
		private bool m_sendResponse;
		private NetBuffer m_response;

		public IPEndPoint Endpoint { get { return m_endPoint; } set { m_endPoint = value; } }
		public NetBuffer Response { get { return m_response; } set { m_response = value; } }
		public bool SendResponse { get { return m_sendResponse; } set { m_sendResponse = value; } }

		public NetDiscoveryRequestEventArgs(IPEndPoint endpoint, NetBuffer response)
		{
			m_endPoint = endpoint;
			m_sendResponse = true;
			m_response = response;
		}
	}

	public sealed class NetStatusChangedEventArgs : EventArgs
	{
		private NetConnection m_connection;
		private NetConnectionStatus m_oldStatus;
		private NetConnectionStatus m_newStatus;
		private string m_reason;

		public NetConnection Connection { get { return m_connection; } }
		public NetConnectionStatus OldStatus { get { return m_oldStatus; } }
		public NetConnectionStatus NewStatus { get { return m_newStatus; } }
		public string Reason { get { return m_reason; } }

		public NetStatusChangedEventArgs(
			NetConnection connection,
			NetConnectionStatus oldStatus,
			NetConnectionStatus newStatus,
			string reason
			)
		{
			m_connection = connection;
			m_oldStatus = oldStatus;
			m_newStatus = newStatus;
			m_reason = reason;
		}
	}

	public sealed class NetReceiptEventArgs : EventArgs
	{
		private NetConnection m_connection;
		private object m_receiptObject;

		public NetConnection Connection { get { return m_connection; } set { m_connection = value; } }
		public object Receipt { get { return m_receiptObject; } set { m_receiptObject = value; } }

		public NetReceiptEventArgs(NetConnection connection, object receiptObject)
		{
			m_connection = connection;
			m_receiptObject = receiptObject;
		}
	}
}
