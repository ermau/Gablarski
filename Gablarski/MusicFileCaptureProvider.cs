// Copyright (c) 2009, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using NAudio.Wave;

namespace Gablarski
{
	[ModuleSelectable (false)]
	public class MusicFileCaptureProvider
		: ICaptureProvider
	{
		private AudioFormat aformat;
		private Mp3FileReader mp3;
		private WaveFileReader wave;
		private WaveStream pcm;

		private WaveStream WaveStream
		{
			get
			{
				return ((WaveStream)mp3 ?? (WaveStream)wave);
			}
		}

		private WaveStream PcmStream
		{
			get { return this.pcm; }
		}

		public MusicFileCaptureProvider (FileInfo file)
		{
			if (file == null)
				throw new ArgumentNullException ("file");
			if (!file.Exists)
				throw new FileNotFoundException ("file not found", file.FullName);
			
			switch (file.Extension.ToLower ())
			{
				case ".mp3":
					this.mp3 = new Mp3FileReader (file.FullName);
					break;

				case ".wav":
					this.wave = new WaveFileReader (file.FullName);
					break;

				default:
					throw new ArgumentException ("Unsupported file format", "file");
			}
		}

		#region ICaptureProvider Members

		public IAudioDevice Device
		{
			get; set;
		}

		public bool IsCapturing
		{
			get;
			private set;
		}

		public bool CanCaptureStereo
		{
			get { return true; }
		}

		public int AvailableSampleCount
		{
			//get { return (int)Math.Round (WaveStream.TotalTime.TotalSeconds, 0) * WaveStream.WaveFormat.SampleRate; }
			get { throw new NotSupportedException (); }
		}

		public void BeginCapture (AudioFormat format)
		{
			IsCapturing = true;

			this.pcm = this.WaveStream;

			if (format == AudioFormat.Mono16Bit || format == AudioFormat.Stereo16Bit)
			{
				if (WaveStream.WaveFormat.BitsPerSample != 16)
					throw new ArgumentException ("format");

				if (WaveStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
					this.pcm = new BlockAlignReductionStream (WaveFormatConversionStream.CreatePcmStream (this.pcm));

				if (format == AudioFormat.Mono16Bit && WaveStream.WaveFormat.Channels != 1)
					this.pcm = new WaveFormatConversionStream (new WaveFormat (44100, 16, (format == AudioFormat.Mono16Bit) ? 1 : 2), this.pcm);
			}
			else if (format == AudioFormat.Mono8Bit || format == AudioFormat.Stereo8Bit)
			{
				if (WaveStream.WaveFormat.BitsPerSample != 8)
					throw new ArgumentException ("format");

				if (WaveStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
					this.pcm = new BlockAlignReductionStream (WaveFormatConversionStream.CreatePcmStream (this.pcm));

				if (format == AudioFormat.Mono8Bit && WaveStream.WaveFormat.Channels != 1)
					this.pcm = new WaveFormatConversionStream (new WaveFormat (44100, 8, (format == AudioFormat.Mono8Bit) ? 1 : 2), this.pcm);
			}

			this.aformat = format;
		}

		public void EndCapture ()
		{
			IsCapturing = false;
		}

		public byte[] ReadSamples ()
		{
			throw new NotSupportedException ();
		}

		public byte[] ReadSamples (int samples)
		{
			return pcm.ReadBytes (this.aformat.GetBytes (samples));
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
			if (this.pcm != null)
			{
				this.pcm.Dispose ();
				this.pcm = null;
			}

			if (this.WaveStream != null)
				this.WaveStream.Dispose ();
		}

		#endregion
	}
}