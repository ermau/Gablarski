using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.Speex
{
	public class SpeexEncoder
	{
		[DllImport ("libspeex.dll")]
		private static extern IntPtr speex_encoder_init (ref SpeexMode mode);

		private IntPtr state;
	}
}