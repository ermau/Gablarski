using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public static class ByteExtensions
	{
		public static byte[] GetBytes (this int self)
		{
			return BitConverter.GetBytes (self);
		}

		public static byte[] GetBytes (this uint self)
		{
			return BitConverter.GetBytes (self);
		}

		public static byte[] GetBytes (this long self)
		{
			return BitConverter.GetBytes (self);
		}

		public static byte[] GetBytes (this ulong self)
		{
			return BitConverter.GetBytes (self);
		}
	}
}