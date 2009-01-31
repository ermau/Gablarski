using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Gablarski.OpenAL
{
	public class Source
		: IDisposable
	{
		internal Source (uint sourceID)
		{
			this.sourceID = sourceID;
		}

		public float Pitch
		{
			get { return GetPropertyF (this.sourceID, SourceProperty.AL_PITCH); }
			set { SetPropertyF (this.sourceID, SourceProperty.AL_PITCH, value); }
		}

		public float Gain
		{
			get { return GetPropertyF (this.sourceID, SourceProperty.AL_GAIN); }
			set { SetPropertyF (this.sourceID, SourceProperty.AL_GAIN, value); }
		}

		public void Delete ()
		{
			uint[] id = new uint[] { this.sourceID };
			alDeleteSources (1, ref id);
			OpenAL.ErrorCheck ();
		}

		private readonly uint sourceID;

		#region IDisposable Members

		public void Dispose ()
		{
			Dispose (true);
		}

		private bool disposed;

		protected void Dispose (bool disposing)
		{
			if (this.disposed)
				return;

			if (!disposing)
				GC.SuppressFinalize (this);

			this.Delete ();
			this.disposed = true;
		}

		~Source ()
		{
			Dispose (false);
		}

		#endregion

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alGenSources (int count, ref uint[] sources);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alDeleteSources (int count, ref uint[] sources);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alGetSourcef (uint sourceID, SourceProperty property, out float value);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void alSourcef (uint sourceID, SourceProperty property, float value);

		internal static float GetPropertyF (uint sourceID, SourceProperty property)
		{
			float value = 0.0f;
			alGetSourcef (sourceID, property, out value);
			OpenAL.ErrorCheck ();

			return value;
		}

		internal static void SetPropertyF (uint sourceID, SourceProperty property, float value)
		{
			alSourcef (sourceID, property, value);
			OpenAL.ErrorCheck ();
		}

		internal static int AvailableSources
		{
			get { return 16; }
		}

		public static Source[] GenerateSources (int count)
		{
			if (count > AvailableSources)
				return null;

			Source[] sources = new Source[count];

			uint[] sourceIDs = new uint[count];
			alGenSources (count, ref sourceIDs);
			OpenAL.ErrorCheck ();

			for (int i = 0; i < count; ++i)
				sources[i] = new Source (sourceIDs[i]);

			return sources;
		}
	}

	enum SourceProperty
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
}