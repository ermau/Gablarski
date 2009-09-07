// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

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