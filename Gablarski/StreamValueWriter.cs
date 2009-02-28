using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Gablarski
{
	public class StreamValueWriter
		: IValueWriter
	{
		public StreamValueWriter (Stream baseStream)
		{
			this.baseStream = baseStream;
			this.AutoFlush = true;
		}

		#region IValueWriter Members
		public bool AutoFlush
		{
			get;
			set;
		}

		public void WriteSByte (sbyte value)
		{
			Write (BitConverter.GetBytes (value));
		}

		public void WriteInt16 (short value)
		{
			Write (BitConverter.GetBytes (value));
		}

		public void WriteInt32 (int value)
		{
			Write (BitConverter.GetBytes (value));
		}

		public void WriteInt64 (long value)
		{
			Write (BitConverter.GetBytes (value));
		}

		public void WriteByte (byte value)
		{
			Write (BitConverter.GetBytes (value));
		}

		public void WriteUInt16 (ushort value)
		{
			Write (BitConverter.GetBytes (value));
		}

		public void WriteUInt32 (uint value)
		{
			Write (BitConverter.GetBytes (value));
		}

		public void WriteUInt64 (ulong value)
		{
			Write (BitConverter.GetBytes (value));
		}

		public void WriteString (string value)
		{
			Write (Encoding.UTF8.GetBytes (value));
		}

		#endregion

		private readonly Stream baseStream;

		private void Write (byte[] buffer)
		{
			baseStream.Write (buffer, 0, buffer.Length);
			
			if (this.AutoFlush)
				baseStream.Flush ();
		}
	}
}