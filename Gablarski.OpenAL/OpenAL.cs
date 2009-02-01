using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;

namespace Gablarski.OpenAL
{
	[SuppressUnmanagedCodeSecurity]
	public static class OpenAL
	{
		static OpenAL ()
		{
			if (GetIsExtensionPresent (null, "ALC_EXT_CAPTURE"))
			{
				OpenAL.IsCaptureSupported = true;
				OpenAL.CaptureDevices = ReadStringsFromMemory (alcGetString (IntPtr.Zero, ALC_CAPTURE_DEVICE_SPECIFIER))
											.Select (n => new CaptureDevice (n));

				OpenAL.DefaultCaptureDevice = new CaptureDevice (Marshal.PtrToStringAnsi (alcGetString (IntPtr.Zero, ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER)));

			}

			if (GetIsExtensionPresent (null, "ALC_ENUMERATION_EXT"))
			{
				OpenAL.PlaybackDevices = ReadStringsFromMemory (alcGetString (IntPtr.Zero, ALC_DEVICE_SPECIFIER))
											.Select (n => new PlaybackDevice (n));
			}

			OpenAL.DefaultPlaybackDevice = new PlaybackDevice (Marshal.PtrToStringAnsi (alcGetString (IntPtr.Zero, ALC_DEFAULT_DEVICE_SPECIFIER)));
		}

		public static DistanceModel DistanceModel
		{
			set
			{
				alDistanceModel (value);
				OpenAL.ErrorCheck ();
			}
		}

		public static bool IsCaptureSupported
		{
			get;
			private set;
		}

		public static PlaybackDevice DefaultPlaybackDevice
		{
			get;
			private set;
		}

		public static IEnumerable<PlaybackDevice> PlaybackDevices
		{
			get;
			private set;
		}

		public static CaptureDevice DefaultCaptureDevice
		{
			get;
			private set;
		}

		public static IEnumerable<CaptureDevice> CaptureDevices
		{
			get;
			private set;
		}

#if DEBUG
		public static bool ErrorChecking = true;
#else
		public static bool ErrorChecking = false;
#endif

		#region AudioFormat Extensions
		public static uint GetBytesPerSample (this AudioFormat self)
		{
			switch (self)
			{
				default:
				case AudioFormat.Mono8Bit:
					return 1;

				case AudioFormat.Mono16Bit:
				case AudioFormat.Stereo8Bit:
					return 2;

				case AudioFormat.Stereo16Bit:
					return 4;
			}
		}

		public static uint GetBytes (this AudioFormat self, uint samples)
		{
			return self.GetBytesPerSample () * samples;
		}

		public static uint GetSamplesPerSecond (this AudioFormat self, uint frequency)
		{
			switch (self)
			{
				default:
				case AudioFormat.Mono8Bit:
				case AudioFormat.Mono16Bit:
					return frequency;

				case AudioFormat.Stereo8Bit:
				case AudioFormat.Stereo16Bit:
					return (frequency * 2);
			}
		}
		#endregion

		internal static IntPtr NullDevice = IntPtr.Zero;

		#region Imports
		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void alcGetIntegerv (IntPtr device, ALCEnum param, int size, out int data);

		[DllImport ("OpenAL32.dll", CallingConvention=CallingConvention.Cdecl)]
		internal static extern IntPtr alcGetString ([In] IntPtr device, int name);

		[DllImport ("OpenAL32.dll", CallingConvention=CallingConvention.Cdecl)]
		internal static extern int alGetError ();

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern sbyte alcIsExtensionPresent ([In] IntPtr device, string extensionName);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern sbyte alIsExtensionPresent (IntPtr device, string extensionName);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void alDistanceModel (DistanceModel model);
		#endregion

		internal static bool GetIsExtensionPresent (Device device, string extension)
		{
			sbyte result = 0;

			IntPtr handle = (device != null) ? device.Handle : IntPtr.Zero;

			if (extension.StartsWith ("ALC"))
				result = alcIsExtensionPresent (handle, extension);
			else
				result = alIsExtensionPresent (handle, extension);

			OpenAL.ErrorCheck ();

			return (result == 1);
		}

		internal static IEnumerable<string> ReadStringsFromMemory (IntPtr location)
		{
			List<string> strings = new List<string> ();

			bool lastNull = false;
			int i = -1;
			byte c;
			while (!((c = Marshal.ReadByte (location, ++i)) == '\0' && lastNull))
			{
				if (c == '\0')
				{
					lastNull = true;

					strings.Add (Marshal.PtrToStringAnsi (location, i));
					location = new IntPtr ((long)location + i + 1);
					i = 0;
				}
				else
					lastNull = false;
			}

			return strings;
		}

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
					throw new ArgumentException ("Invalid Enum");

				case OpenALError.AL_INVALID_NAME:
					throw new ArgumentException ("Invalid Name");

				case OpenALError.AL_INVALID_VALUE:
					throw new ArgumentException ("Invalid Value");

				case OpenALError.AL_INVALID_OPERATION:
					throw new InvalidOperationException ();
			}
		}

		internal const int ALC_DEFAULT_DEVICE_SPECIFIER = 0x1004;
		internal const int ALC_DEVICE_SPECIFIER = 0x1005;
		internal const int ALC_CAPTURE_DEVICE_SPECIFIER = 0x310;
		internal const int ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER = 0x311;
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

	internal enum ALCEnum
	{
		ALC_MAJOR_VERSION	= 0x1000,
		ALC_MINOR_VERSION	= 0x1001,
		ALC_ATTRIBUTES_SIZE	= 0x1002,
		ALC_ALL_ATTRIBUTES	= 0x1003,
		ALC_CAPTURE_SAMPLES	= 0x312
	}

	public enum DistanceModel
	{
		None = 0
	}

	public enum AudioFormat
	{
		Mono8Bit = 0x1100,
		Mono16Bit = 0x1101,
		Stereo8Bit = 0x1102,
		Stereo16Bit = 0x1103
	}
}