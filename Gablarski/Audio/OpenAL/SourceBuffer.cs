using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;

namespace Gablarski.Audio.OpenAL
{
	[SuppressUnmanagedCodeSecurity]
	public class SourceBuffer
		: IDisposable
	{
		internal SourceBuffer (uint bufferID)
		{
			this.bufferID = bufferID;
		}

		public void Buffer (byte[] data, AudioFormat format, uint frequency)
		{
			alBufferData (this.bufferID, format, data, data.Length, frequency);
			OpenAL.ErrorCheck ();
		}

		#region IDisposable Members
		private bool disposed;

		public void Dispose ()
		{
			this.Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (this.disposed)
				return;

			this.disposed = true;

			alDeleteBuffers (1, new[] { this.bufferID });
			OpenAL.ErrorCheck ();

			Buffers.Remove (this.bufferID);
		}

		#endregion

		internal readonly uint bufferID;

		public static SourceBuffer Generate ()
		{
			return Generate (1)[0];
		}

		public static SourceBuffer[] Generate (int count)
		{
			if (Buffers == null)
				Buffers = new Dictionary<uint, SourceBuffer> (count);

			SourceBuffer[] buffers = new SourceBuffer[count];

			uint[] bufferIDs = new uint[count];
			alGenBuffers (count, bufferIDs);
			Audio.OpenAL.OpenAL.ErrorCheck ();

			for (int i = 0; i < count; ++i)
			{
				buffers[i] = new SourceBuffer (bufferIDs[i]);
				Buffers.Add (buffers[i].bufferID, buffers[i]);
			}

			return buffers;
		}

		#region Imports
		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alGenBuffers (int count, uint[] bufferIDs);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alBufferData (uint bufferID, AudioFormat format, byte[] data, int byteSize, uint frequency);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alDeleteBuffers (int numBuffers, uint[] bufferIDs);
		#endregion

		internal static Dictionary<uint, SourceBuffer> Buffers
		{
			get;
			set;
		}
	}
}