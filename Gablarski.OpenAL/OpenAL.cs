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
											.Select (n => new Device (n));
			}
			
			if (GetIsExtensionPresent (null, "ALC_ENUMERATION_EXT"))
				OpenAL.PlaybackDevices = ReadStringsFromMemory (alcGetString (IntPtr.Zero, ALC_DEVICE_SPECIFIER))
											.Select (n => new Device (n));
		}

		public static bool IsCaptureSupported
		{
			get;
			private set;
		}

		public static IEnumerable<Device> PlaybackDevices
		{
			get;
			private set;
		}

		public static IEnumerable<Device> CaptureDevices
		{
			get;
			private set;
		}

		public static bool ErrorChecking = true;

		internal static IntPtr NullDevice = IntPtr.Zero;

		[DllImport ("OpenAL32.dll", CallingConvention=CallingConvention.Cdecl)]
		internal static extern IntPtr alcGetString ([In] IntPtr device, int name);

		[DllImport ("OpenAL32.dll")]
		internal static extern int alGetError ();

		[DllImport ("OpenAL32.dll")]
		internal static extern sbyte alcIsExtensionPresent ([In] IntPtr device, string extensionName);

		[DllImport ("OpenAL32.dll")]
		internal static extern sbyte alIsExtensionPresent (IntPtr device, string extensionName);

		internal static bool GetIsExtensionPresent (Device device, string extension)
		{
			sbyte result = 0;

			IntPtr handle = (device != null) ? device.Handle : IntPtr.Zero;

			if (extension.StartsWith ("ALC"))
				result = alcIsExtensionPresent (handle, extension);
			else
				result = alIsExtensionPresent (handle, extension);

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

		internal const int ALC_DEVICE_SPECIFIER = 0x1005;
		internal const int ALC_CAPTURE_DEVICE_SPECIFIER = 0x310;
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
}