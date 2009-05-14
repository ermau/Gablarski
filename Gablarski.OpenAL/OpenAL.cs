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
			#if DEBUG
			OpenAL.ErrorChecking = true;
			#endif

			if (GetIsExtensionPresent (null, "ALC_EXT_CAPTURE"))
			{
				OpenAL.IsCaptureSupported = true;
				
				string defaultName = Marshal.PtrToStringAnsi (alcGetString (IntPtr.Zero, ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER));

				var strings = ReadStringsFromMemory (alcGetString (IntPtr.Zero, ALC_CAPTURE_DEVICE_SPECIFIER));
				CaptureDevice[] devices = new CaptureDevice[strings.Length];
				for (int i = 0; i < strings.Length; ++i)
				{
					string s = strings[i];
					devices[i] = new CaptureDevice (s);

					if (s == defaultName)
						OpenAL.DefaultCaptureDevice = devices[i];
				}
				
				OpenAL.CaptureDevices = devices;

				if (OpenAL.DefaultCaptureDevice == null)
					OpenAL.DefaultCaptureDevice = new CaptureDevice (defaultName);
			}

			OpenAL.DefaultPlaybackDevice = new PlaybackDevice (Marshal.PtrToStringAnsi (alcGetString (IntPtr.Zero, ALC_DEFAULT_DEVICE_SPECIFIER)));

			if (GetIsExtensionPresent (null, "ALC_ENUMERATE_ALL_EXT"))
			{
				string defaultName = Marshal.PtrToStringAnsi (alcGetString (IntPtr.Zero, ALC_DEFAULT_ALL_DEVICES_SPECIFIER));

				var strings = ReadStringsFromMemory (alcGetString (IntPtr.Zero, ALC_ALL_DEVICES_SPECIFIER));
				PlaybackDevice[] devices = new PlaybackDevice[strings.Length];
				for (int i = 0; i < strings.Length; ++i)
				{
					string s = strings[i];
					devices[i] = new PlaybackDevice (s);

					if (s == defaultName)
						OpenAL.DefaultPlaybackDevice = devices[i];
				}

				OpenAL.PlaybackDevices = devices;

				if (OpenAL.DefaultPlaybackDevice == null)
					OpenAL.DefaultPlaybackDevice = new PlaybackDevice (defaultName);
			}
			else if (GetIsExtensionPresent (null, "ALC_ENUMERATION_EXT"))
			{
				string defaultName = Marshal.PtrToStringAnsi (alcGetString (IntPtr.Zero, ALC_DEFAULT_DEVICE_SPECIFIER));

				var strings = ReadStringsFromMemory (alcGetString (IntPtr.Zero, ALC_DEVICE_SPECIFIER));
				PlaybackDevice[] devices = new PlaybackDevice[strings.Length];
				for (int i = 0; i < strings.Length; ++i)
				{
					string s = strings[i];
					devices[i] = new PlaybackDevice (s);

					if (s == defaultName)
						OpenAL.DefaultPlaybackDevice = devices[i];
				}

				OpenAL.PlaybackDevices = devices;

				if (OpenAL.DefaultPlaybackDevice == null)
					OpenAL.DefaultPlaybackDevice = new PlaybackDevice (defaultName);
			}
		}


		/// <summary>
		/// Sets the distance model for OpenAL.
		/// </summary>
		public static DistanceModel DistanceModel
		{
			set
			{
				alDistanceModel (value);
				OpenAL.ErrorCheck ();
			}
		}

		/// <summary>
		/// Sets the speed of sound for OpenAL.
		/// </summary>
		public static float SpeedOfSound
		{
			set
			{
				alSpeedOfSound (value);
				OpenAL.ErrorCheck ();
			}
		}

		/// <summary>
		/// Sets the doppler factor for OpenAL.
		/// </summary>
		public static float DopplerFactor
		{
			set
			{
				alDopplerFactor (value);
				OpenAL.ErrorCheck ();
			}
		}

		/// <summary>
		/// Gets whether capture support is available.
		/// </summary>
		public static bool IsCaptureSupported
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the default playback device.
		/// </summary>
		public static PlaybackDevice DefaultPlaybackDevice
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a listing of playback devices.
		/// </summary>
		public static IEnumerable<PlaybackDevice> PlaybackDevices
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the default capture device. <c>null</c> if unsupported.
		/// </summary>
		public static CaptureDevice DefaultCaptureDevice
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a listing of capture devices. <c>null</c> if unsupported.
		/// </summary>
		public static IEnumerable<CaptureDevice> CaptureDevices
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets whether error checking is enabled.
		/// </summary>
		public static bool ErrorChecking
		{
			get;
			private set;
		}

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
		internal static extern void alDopplerFactor (float value);

		[DllImport ("OpenAL32.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void alSpeedOfSound (float value);

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
			IntPtr handle = (device != null) ? device.Handle : IntPtr.Zero;

			sbyte result = extension.StartsWith("ALC")
							? alcIsExtensionPresent(handle, extension)
							: alIsExtensionPresent(handle, extension);

			OpenAL.ErrorCheck ();

			return (result == 1);
		}

		internal static string[] ReadStringsFromMemory (IntPtr location)
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

			return strings.ToArray();
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
		internal const int ALC_ALL_DEVICES_SPECIFIER = 0x1013;
		internal const int ALC_DEFAULT_ALL_DEVICES_SPECIFIER = 0x1012;
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