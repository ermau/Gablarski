using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IValueWriter
	{
		void WriteSByte (SByte value);
		void WriteInt16 (Int16 value);
		void WriteInt32 (Int32 value);
		void WriteInt64 (Int64 value);
		
		void WriteByte (Byte value);
		void WriteUInt16 (UInt16 value);
		void WriteUInt32 (UInt32 value);
		void WriteUInt64 (UInt64 value);

		void WriteString (string value);
	}
}