using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.Speex
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct SpeexBits
	{
		IntPtr chars;
		int nbBits;
		IntPtr charPtr;
		IntPtr bitPtr;
		int owner;
		int overflow;
		int buf_size;
		int reserved1;
		IntPtr reserved2;
	}
}