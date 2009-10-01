using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Audio;

namespace Gablarski
{
	[ModuleSelectable (false)]
	public class MusicFileCaptureProvider
		: ICaptureProvider
	{
		private readonly FileInfo file;

		public MusicFileCaptureProvider (FileInfo file)
		{
			if (file == null)
				throw new ArgumentNullException ("file");
			if (!file.Exists)
				throw new ArgumentException ("file doesn't exist", "file");

			this.file = file;
		}

		#region ICaptureProvider Members

		public event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;

		public IAudioDevice Device
		{
			get; set;
		}

		public bool IsCapturing
		{ 
			get { throw new NotImplementedException (); }
		}

		public bool CanCaptureStereo
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		public int AvailableSampleCount
		{
			get { throw new NotImplementedException (); }
		}

		public void BeginCapture (AudioFormat format)
		{
			throw new NotImplementedException ();
		}

		public void EndCapture ()
		{
			throw new NotImplementedException ();
		}

		public byte[] ReadSamples ()
		{
			throw new NotImplementedException ();
		}

		public byte[] ReadSamples (int samples)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region IAudioDeviceProvider Members

		public IEnumerable<IAudioDevice> GetDevices ()
		{
			return Enumerable.Empty<IAudioDevice>();
		}

		public IAudioDevice DefaultDevice
		{
			get { throw new NotSupportedException (); }
		}

		#endregion

		#region IDisposable Members

		public void Dispose ()
		{
		}

		#endregion
	}
}