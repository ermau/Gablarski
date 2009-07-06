using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;

namespace Gablarski.OpenAL
{
	[SuppressUnmanagedCodeSecurity]
	public class Source
		: IDisposable
	{
		internal Source (uint sourceID)
		{
			this.sourceID = sourceID;
		}

		public SourceState State
		{
			get
			{
				int state;
				alGetSourcei (this.sourceID, IntSourceProperty.AL_SOURCE_STATE, out state);
				OpenAL.ErrorCheck ();

				return (SourceState)state;
			}
		}

		public bool IsPlaying
		{
			get { return (this.State == SourceState.Playing); }
		}

		public bool IsPaused
		{
			get { return (this.State == SourceState.Paused); }
		}

		public bool IsStopped
		{
			get { return (this.State == SourceState.Stopped); }
		}

		public int ProcessedBuffers
		{
			get
			{
				int buffers;
				alGetSourcei (this.sourceID, IntSourceProperty.AL_BUFFERS_PROCESSED, out buffers);
				OpenAL.ErrorCheck();

				return buffers;
			}
		}

		public float Pitch
		{
			get { return GetPropertyF (this.sourceID, FloatSourceProperty.AL_PITCH); }
			set { SetPropertyF (this.sourceID, FloatSourceProperty.AL_PITCH, value); }
		}

		/// <summary>
		/// Gets or sets the minimum gain for this source.
		/// </summary>
		public float MinimumGain
		{
			get { return GetPropertyF (this.sourceID, FloatSourceProperty.AL_MIN_GAIN); }
			set { SetPropertyF (this.sourceID, FloatSourceProperty.AL_MIN_GAIN, value); }
		}

		/// <summary>
		/// Gets or sets the source's gain.
		/// </summary>
		public float Gain
		{
			get { return GetPropertyF (this.sourceID, FloatSourceProperty.AL_GAIN); }
			set { SetPropertyF (this.sourceID, FloatSourceProperty.AL_GAIN, value); }
		}

		/// <summary>
		/// Gets or sets the maximum gain for this source.
		/// </summary>
		public float MaximumGain
		{
			get { return GetPropertyF (this.sourceID, FloatSourceProperty.AL_MAX_GAIN); }
			set { SetPropertyF (this.sourceID, FloatSourceProperty.AL_MAX_GAIN, value); }
		}

		public void Queue (SourceBuffer buffer)
		{
			alSourceQueueBuffers (this.sourceID, 1, new [] { buffer.bufferID });
			OpenAL.ErrorCheck ();
		}

		public void QueueAndPlay (SourceBuffer buffer)
		{
			this.Queue (buffer);
			this.Play ();
		}

		public void Queue (IEnumerable<SourceBuffer> buffers)
		{
			uint[] bufferIDs = buffers.Select (b => b.bufferID).ToArray ();
			alSourceQueueBuffers (this.sourceID, bufferIDs.Length, bufferIDs);
			OpenAL.ErrorCheck ();
		}

		public SourceBuffer[] Dequeue ()
		{
			return Dequeue (this.ProcessedBuffers);
		}

		public SourceBuffer[] Dequeue (int buffers)
		{
			uint[] bufferIDs = new uint[buffers];
			alSourceUnqueueBuffers (this.sourceID, buffers, bufferIDs);
			OpenAL.ErrorCheck ();

			SourceBuffer[] dequeued = new SourceBuffer[bufferIDs.Length];
			for (int i = 0; i < bufferIDs.Length; ++i)
				dequeued[i] = SourceBuffer.GetBuffer (bufferIDs[i]);

			return dequeued;
		}

		public void Play ()
		{
			PlayCore (true);
		}

		public void Replay ()
		{
			PlayCore (false);
		}

		public void Pause ()
		{
			alSourcePause (this.sourceID);
			OpenAL.ErrorCheck ();
		}

		public void Stop ()
		{
			alSourceStop (this.sourceID);
			OpenAL.ErrorCheck ();
		}

		private readonly uint sourceID;

		protected void PlayCore (bool check)
		{
			if (check && this.IsPlaying)
				return;

			alSourcePlay (this.sourceID);
			OpenAL.ErrorCheck ();
		}

		#region IDisposable Members

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		private bool disposed;

		protected void Dispose (bool disposing)
		{
			if (this.disposed)
				return;

			uint[] id = new [] { this.sourceID };
			alDeleteSources (1, id);
			//OpenAL.ErrorCheck ();

			this.disposed = true;
		}

		~Source ()
		{
			Dispose (false);
		}

		#endregion

		public static Source Generate ()
		{
			return Generate (1)[0];
		}

		public static Source[] Generate (int count)
		{
			if (count > AvailableSources)
				return null;

			Source[] sources = new Source[count];

			uint[] sourceIDs = new uint[count];
			alGenSources (count, sourceIDs);
			OpenAL.ErrorCheck ();

			for (int i = 0; i < count; ++i)
				sources[i] = new Source (sourceIDs[i]);

			return sources;
		}
		// ReSharper disable InconsistentNaming
		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alGetSourcei (uint sourceID, IntSourceProperty property, out int value);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alSourcePlay (uint sourceID);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alSourcePause (uint sourceID);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alSourceStop (uint sourceID);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alSourceQueueBuffers (uint sourceID, int number, uint[] bufferIDs);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alSourceUnqueueBuffers (uint sourceID, int buffers, uint[] buffersDequeued);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alGenSources (int count, uint[] sources);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alDeleteSources (int count, uint[] sources);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alGetSourcef (uint sourceID, FloatSourceProperty property, out float value);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alSourcef (uint sourceID, FloatSourceProperty property, float value);
		// ReSharper restore InconsistentNaming

		internal static float GetPropertyF (uint sourceID, FloatSourceProperty property)
		{
			float value;
			alGetSourcef (sourceID, property, out value);
			OpenAL.ErrorCheck ();

			return value;
		}

		internal static void SetPropertyF (uint sourceID, FloatSourceProperty property, float value)
		{
			alSourcef (sourceID, property, value);
			OpenAL.ErrorCheck ();
		}

		internal static int AvailableSources
		{
			get { return 16; }
		}
	}

	public enum SourceState
	{
		Initial = 0x1011,
		Playing = 0x1012,
		Paused = 0x1013,
		Stopped = 0x1014
	}

	// ReSharper disable InconsistentNaming
	internal enum IntSourceProperty
	{
		AL_SOURCE_STATE = 0x1010,
		AL_BUFFERS_QUEUED = 0x1015,
		AL_BUFFERS_PROCESSED = 0x1016
	}

	internal enum FloatSourceProperty
	{
		AL_PITCH				= 0x1003,
		AL_GAIN					= 0x100A,
		AL_MIN_GAIN				= 0x100D,
		AL_MAX_GAIN				= 0x100E,
		AL_MAX_DISTANCE			= 0x1023,
		AL_ROLLOFF_FACTOR		= 0x1021,
		AL_CONE_OUTER_GAIN		= 0x1022,
		AL_CONE_INNER_ANGLE		= 0x1001,
		AL_CONE_OUTER_ANGLE		= 0x1002,
		AL_REFERENCE_DISTANCE	= 0x1020
	}
	// ReSharper restore InconsistentNaming
}