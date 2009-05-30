using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.CELT
{
	internal enum ErrorCode
	{
		OK = 0,
		BAD_ARG = -1,
		INVALID_MODE = -2,
		INTERNAL_ERROR = -3,
		CORRUPTED_DATA = -4,
		UNIMPLEMENTED = -5
	}

	internal enum Request
	{
		SET_COMPLEXITY_REQUEST = 2,
		SET_LTP_REQUEST = 3,
		
		GET_FRAME_SIZE = 1000,
		GET_LOOKAHEAD = 1001,
		GET_NB_CHANNELS = 1002,
		
		GET_BITSTREAM_VERSION = 2000
	}

	internal static class ErrorCodeExtensions
	{
		public static void ThrowIfError (this ErrorCode self)
		{
			switch (self)
			{
				case ErrorCode.BAD_ARG:
					throw new ArgumentException ();

				case ErrorCode.UNIMPLEMENTED:
					throw new NotImplementedException ();

				case ErrorCode.INVALID_MODE:
					throw new ArgumentException ("mode is invalid", "mode");

				case ErrorCode.CORRUPTED_DATA:
					throw new ArgumentException ("Data passed is corrupted");

				case ErrorCode.INTERNAL_ERROR:
					throw new Exception ("Internal error.");
			}
		}
	}
}