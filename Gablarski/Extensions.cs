using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Gablarski
{
	public static class Extensions
	{
		public static byte[] ReadBytes (this Stream stream, int size)
		{
			byte[] buffer = new byte[size];

			int i = 0;
			int bytes = 0;
			while (i < buffer.Length && (bytes = stream.Read (buffer, i, size)) > 0)
			{
				i += bytes;
				size -= bytes;
			}

			return buffer;
		}

		public static bool IsEmpty (this string self)
		{
			return (String.IsNullOrEmpty (self) || self.Trim () == String.Empty);
		}
	}
}