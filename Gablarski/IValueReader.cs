using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono.Rocks
{
	public interface IValueReader
	{
		IValueReader Read (out sbyte value);
		IValueReader Read (out short value);
		IValueReader Read (out int value);
		IValueReader Read (out long value);

		IValueReader Read (out byte value);
		IValueReader Read (out ushort value);
		IValueReader Read (out uint value);
		IValueReader Read (out ulong value);
	}
}