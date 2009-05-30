using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.CELT
{
	public class CeltDecoder
		: IDisposable
	{
		private IntPtr decoderState;

		#region IDisposable Members
		public void Dispose ()
		{
			GC.SuppressFinalize (this);
			Dispose (true);
		}

		private bool disposed;
		protected void Dispose (bool disposing)
		{
			if (this.disposed)
				return;



			this.disposed = true;
		}
		#endregion

		[DllImport ("libcert.dll")]
		[return: MarshalAs (UnmanagedType.SysInt)]
		private static extern ErrorCode celt_decode (IntPtr decoderState, byte[] data, int length, out short[] pcm);
	}
}