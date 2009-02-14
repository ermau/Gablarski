using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono.Rocks
{
	public interface IValueWriter
	{
		IValueWriter Write (sbyte value);
		IValueWriter Write (short value);
		IValueWriter Write (int value);
		IValueWriter Write (long value);

		IValueWriter Write (byte value);
		IValueWriter Write (ushort value);
		IValueWriter Write (uint value);
		IValueWriter Write (ulong value);
	}
}