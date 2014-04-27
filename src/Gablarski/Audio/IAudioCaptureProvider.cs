// Copyright (c) 2011, Eric Maupin
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
using System.Linq;

namespace Gablarski.Audio
{
	public interface IAudioCaptureProvider
		: IAudioDeviceProvider
	{
		event EventHandler<SamplesAvailableEventArgs> SamplesAvailable;

		/// <summary>
		/// Gets or sets the device to capture from.
		/// </summary>
		IAudioDevice Device { get; set; }

		/// <summary>
		/// Gets whether or not this provider is currently capturing audio.
		/// </summary>
		bool IsCapturing { get; }

		/// <summary>
		/// Gets an enumeration of supported formats.
		/// </summary>
		IEnumerable<AudioFormat> SupportedFormats { get; }

		/// <summary>
		/// Begins a capture.
		/// </summary>
		/// <param name="format">The format to capture in.</param>
		/// <param name="frameSize">The size of frames.</param>
		/// <exception cref="InvalidOperationException">If <see cref="Device"/> is null.</exception>
		/// <exception cref="NotSupportedException"><paramref name="format"/> is an unsupported format.</exception>
		/// <remarks>
		/// <paramref name="frameSize"/> may affect how often <see cref="SamplesAvailable"/> is raised.
		/// </remarks>
		void BeginCapture (AudioFormat format, int frameSize);

		/// <summary>
		/// Reads <paramref name="count"/> samples, waits if neccessary.
		/// </summary>
		/// <param name="count">The number of samples to read.</param>
		/// <returns>A byte array of the samples.</returns>
		byte[] ReadSamples (int count);

		/// <summary>
		/// Ends a capture.
		/// </summary>
		/// <exception cref="InvalidOperationException">If <see cref="Device"/> is null.</exception>
		void EndCapture ();
	}

	/// <summary>
	/// Provides data for a samples available event.
	/// </summary>
	public class SamplesAvailableEventArgs
		: EventArgs
	{
		/// <summary>
		/// Constructs a new <see cref="SamplesAvailableEventArgs"/>
		/// </summary>
		/// <param name="provider">The provider samples are available from.</param>
		/// <param name="available">The number of available samples.</param>
		public SamplesAvailableEventArgs (IAudioCaptureProvider provider, int available)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			Provider = provider;
			Available = available;
		}

		public IAudioCaptureProvider Provider
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the number of available samples.
		/// </summary>
		public int Available
		{
			get;
			private set;
		}
	}
}