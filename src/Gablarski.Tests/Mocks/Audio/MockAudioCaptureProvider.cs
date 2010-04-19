using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;

namespace Gablarski.Tests.Mocks.Audio
{
	public class MockAudioCaptureProvider
		: IAudioCaptureProvider
	{
		private int frameSize = 256;
		public int FrameSize
		{
			get { return this.frameSize; }
			set { this.frameSize = value; }
		}

		#region Implementation of IAudioDeviceProvider

		/// <summary>
		/// Gets a listing of devices for this provider.
		/// </summary>
		/// <returns>A listing of devices to choose from.</returns>
		public IEnumerable<IAudioDevice> GetDevices()
		{
			return new[] { captureDevice };
		}

		/// <summary>
		/// Gets the default device for this provider.
		/// </summary>
		public IAudioDevice DefaultDevice
		{
			get { return captureDevice; }
		}

		public IEnumerable<AudioFormat> SupportedFormats
		{
			get
			{
				return new[]
				{
					AudioFormat.Mono8Bit,
					AudioFormat.Mono16Bit,
					AudioFormat.Stereo8Bit,
					AudioFormat.Stereo16Bit,
				};
			}
		}

		public int AvailableSampleCount
		{
			 get { return this.frameSize; }
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
		}

		#endregion

		#region Implementation of IAudioCaptureProvider

		public IAudioDevice Device
		{
			get; set;
		}

		public bool IsCapturing { get; private set; }

		public void BeginCapture (int frequency, AudioFormat format)
		{
			this.IsCapturing = true;
		}

		public void EndCapture()
		{
			this.IsCapturing = false;
		}

		public byte[] ReadSamples()
		{
			return new byte[this.frameSize];
		}

		public byte[] ReadSamples(int samples)
		{
			return new byte[samples];
		}

		#endregion

		private readonly MockAudioDevice captureDevice = new MockAudioDevice ("MockCaptureDevice");
	}
}