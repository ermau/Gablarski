using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Gablarski.Audio.Speex
{
	public class SpeexJitterBuffer
		: IDisposable
	{
		public SpeexJitterBuffer (int span)
		{
			this.state = jitter_buffer_init (span);
		}

		public int AvailableCount
		{
			get { return GetValue (JITTER_BUFFER.GET_AVAILABLE_COUNT); }
		}

		public unsafe void Push (SpeexJitterBufferPacket packet)
		{
			fixed (byte* pBuffer = packet.Data)
			{
				JitterBufferPacket p = new JitterBufferPacket();
				p.data = pBuffer;
				p.len = (uint)packet.Data.Length;
				p.sequence = (ushort)packet.Sequence;
				p.span = packet.Span;
				p.timestamp = packet.TimeStamp;

				lock (sync)
				{
					jitter_buffer_put (this.state, &p);
				}
			}
		}

		public unsafe SpeexJitterBufferPacket Pull (int span)
		{
			int offset;

			byte[] buffer = new byte[4096];

			var p = new JitterBufferPacket();
			JitterBufferStatus result;
			fixed (byte* pbuffer = buffer)
			{
				p.data = pbuffer;
				p.len = (uint)buffer.Length;
				lock (sync)
				{
					result = jitter_buffer_get (this.state, &p, span, out offset);
				}
			}

			switch (result)
			{
				case JitterBufferStatus.BadArgument:
					throw new ArgumentException();

				case JitterBufferStatus.InternalError:
					throw new Exception();

				default:
					break;
			}

			var sp = new SpeexJitterBufferPacket();
			sp.Encoded = (result != JitterBufferStatus.Missing);
			sp.Sequence = p.sequence;
			sp.Span = p.span;
			sp.TimeStamp = p.timestamp;
			sp.Data = new byte[p.len];
			if (sp.Encoded)		
				Array.Copy (buffer, sp.Data, p.len);

			return sp;
		}

		public void Tick()
		{
			lock (sync)
			{
				jitter_buffer_tick (this.state);
			}
		}

		public unsafe void UpdateDelay (SpeexJitterBufferPacket packet)
		{
			lock (sync)
			{
				fixed (byte* pBuffer = packet.Data)
				{
					JitterBufferPacket p = new JitterBufferPacket();
					p.data = pBuffer;
					p.len = (uint)packet.Data.Length;
					p.sequence = (ushort)packet.Sequence;
					p.span = packet.Span;
					p.timestamp = packet.TimeStamp;

					lock (sync)
					{
						jitter_buffer_update_delay (this.state, &p, IntPtr.Zero);
					}
				}
			}
		}

		#region IDisposable Members
		public void Dispose ()
		{
			GC.SuppressFinalize (this);
			Dispose (true);
		}

		private bool disposed;
		protected virtual void Dispose (bool disposing)
		{
			if (this.disposed)
				return;

			if (this.state != IntPtr.Zero)
			{
				lock (sync)
				{
					jitter_buffer_destroy (this.state);
				}
	
				this.state = IntPtr.Zero;
			}
			
			this.disposed = true;
		}

		~SpeexJitterBuffer ()
		{
			Dispose (false);
		}
		#endregion

		private IntPtr state;
		private readonly object sync = new object();

		private int GetValue (JITTER_BUFFER param)
		{
			int requestState;
			int value = 0;
			lock (sync)
			{
				 requestState = jitter_buffer_ctl (this.state, (int)param, ref value);
			}

			if (requestState == -1)
				throw new ArgumentException ("Unknown request " + param);

			return value;
		}

		// ReSharper disable InconsistentNaming
		[StructLayout (LayoutKind.Sequential)]
		internal unsafe struct JitterBufferPacket
		{
			public byte* data;
			public uint len;
			public uint timestamp;
			public uint span;
			public ushort sequence;
			public uint user_data;
		}

		private enum JitterBufferStatus
		{
			Ok = 0,
			Missing = 1,
			Insertion = 2,
			InternalError = -1,
			BadArgument = -2
		}

		private enum JITTER_BUFFER
		{
			SET_MARGIN = 0,
			GET_MARGIN = 1,
			GET_AVAILABLE_COUNT = 3,
			SET_DESTROY_CALLBACK = 4,
			GET_DESTROY_CALLBACK = 5,
			SET_DELAY_STEP = 6,
			GET_DELAY_STEP = 7,
			SET_CONCEALMENT_SIZE = 8,
			GET_CONCEALMENT_SIZE = 9,
			SET_MAX_LATE_RATE = 10,
			GET_MAX_LATE_RATE = 11,
			SET_LATE_COST = 12,
			GET_LATE_COST = 13,
		}

		[DllImport ("libspeexdsp.dll")]
		private static extern IntPtr jitter_buffer_init (int step_size);

		[DllImport ("libspeexdsp.dll")]
		private static extern void jitter_buffer_reset (IntPtr state);

		[DllImport ("libspeexdsp.dll")]
		private static extern void jitter_buffer_destroy (IntPtr state);

		[DllImport ("libspeexdsp.dll")]
		private static extern int jitter_buffer_ctl (IntPtr state, int request, ref int data);

		[DllImport ("libspeexdsp.dll")]
		private static extern void jitter_buffer_tick (IntPtr state);

		[DllImport ("libspeexdsp.dll")]
		private unsafe static extern void jitter_buffer_put (IntPtr state, JitterBufferPacket* packet);

		[DllImport ("libspeexdsp.dll")]
		private unsafe static extern JitterBufferStatus jitter_buffer_get (IntPtr state, JitterBufferPacket* packet, int desired_span, out int offset);

		[DllImport ("libspeexdsp.dll")]
		private static extern unsafe JitterBufferStatus jitter_buffer_update_delay (IntPtr state, JitterBufferPacket* packet, IntPtr offset);
		// ReSharper restore InconsistentNaming
	}
}