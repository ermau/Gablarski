using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net;

namespace Lidgren.Network
{
	internal sealed class NetMessage
	{
		internal NetMessageType m_msgType;

		internal NetMessageLibraryType m_type;
		internal NetChannel m_sequenceChannel;
		internal int m_sequenceNumber = -1;
	
		internal NetBuffer m_data;
		internal NetConnection m_sender;
		internal IPEndPoint m_senderEndPoint;

		internal int m_numSent;
		internal double m_nextResend;

		internal NetBuffer m_receiptData;

		public NetMessage()
		{
			m_msgType = NetMessageType.Data;
		}

		internal void Encode(NetBuffer intoBuffer)
		{
			Debug.Assert(m_sequenceNumber != -1);

			// message type, netchannel and sequence number
			intoBuffer.Write((byte)((int)m_type | ((int)m_sequenceChannel << 3)));
			intoBuffer.Write((ushort)m_sequenceNumber);

			// payload length
			int len = m_data.LengthBytes;
			intoBuffer.WriteVariableUInt32((uint)len);

			// copy payload
			intoBuffer.Write(m_data.Data, 0, len);

			return;
		}

		/// <summary>
		/// Read this message from the packet buffer
		/// </summary>
		/// <returns>new read pointer position</returns>
		internal void ReadFrom(NetBuffer buffer, IPEndPoint endpoint)
		{
			m_senderEndPoint = endpoint;

			// read header
			byte header = buffer.ReadByte();
			m_type = (NetMessageLibraryType)(header & 7);
			m_sequenceChannel = (NetChannel)(header >> 3);
			m_sequenceNumber = buffer.ReadUInt16();

			int payLen = (int)buffer.ReadVariableUInt32();

			// copy payload
			m_data.Reset();
			m_data.Write(buffer.ReadBytes(payLen)); // TODO: optimize

			return;
		}

		public override string ToString()
		{
			if (m_type == NetMessageLibraryType.System)
				return "[" + (NetSystemType)m_data.Data[0] + " " + m_sequenceChannel + "|" + m_sequenceNumber + "]";

			return "[" + m_type + " " + m_sequenceChannel + "|" + m_sequenceNumber + "]";
		}
	}
}
