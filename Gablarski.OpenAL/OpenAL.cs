using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.OpenAL
{
	public static class OpenAL
	{
		public static bool ErrorChecking = true;

		[DllImport ("OpenAL32.dll")]
		internal static extern int alGetError ();

		internal static void ErrorCheck ()
		{
			if (!ErrorChecking)
				return;

			int err = alGetError ();
			switch ((OpenALError)err)
			{
				case OpenALError.AL_NO_ERROR:
					return;

				case OpenALError.AL_OUT_OF_MEMORY:
					throw new OutOfMemoryException ();

				case OpenALError.AL_INVALID_ENUM:
				case OpenALError.AL_INVALID_NAME:
				case OpenALError.AL_INVALID_VALUE:
					throw new ArgumentException ();

				case OpenALError.AL_INVALID_OPERATION:
					throw new InvalidOperationException ();
			}
		}
	}

	internal enum OpenALError
	{
		AL_NO_ERROR = 0,
		AL_INVALID_NAME = 0xA001,
		AL_INVALID_ENUM = 0xA002,
		AL_INVALID_VALUE = 0xA003,
		AL_INVALID_OPERATION = 0xA004,
		AL_OUT_OF_MEMORY = 0xA005,
	}
}