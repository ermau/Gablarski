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
using System.Linq;
using System.Text;

namespace Gablarski.Audio.OpenAL.Providers
{
	public class OpenALPlaybackProvider
		: IPlaybackProvider
	{
		public OpenALPlaybackProvider()
		{
			this.pool.SourceFinished += PoolSourceFinished;
		}

		#region IPlaybackProvider Members
		public event EventHandler<SourceFinishedEventArgs> SourceFinished;

		public IAudioDevice Device
		{
			get { return this.device; }
			set
			{
				this.device = (value as PlaybackDevice);
				if (this.device == null)
					throw new ArgumentException ("Can only accept OpenAL PlaybackDevice devices");
			}
		}

		public void QueuePlayback (AudioSource audioSource, byte[] data)
		{
			if (!this.device.IsOpen)
				this.device.Open();

			if (this.context == null)
				this.context = this.device.CreateAndActivateContext();

			Stack<SourceBuffer> bufferStack;
			if (!this.buffers.TryGetValue (audioSource, out bufferStack))
				this.buffers[audioSource] = bufferStack = new Stack<SourceBuffer>();

			Source source = this.pool.RequestSource (audioSource);

			const int bufferLen = 6;

			if (data.Length == 0)
				return;

			if (!source.IsPlaying)
			{
				RequireBuffers (bufferStack, source, bufferLen);
				for (int i = 0; i < bufferLen; ++i)
				{
					SourceBuffer wait = bufferStack.Pop();
					wait.Buffer (new byte[audioSource.FrameSize * 2 * audioSource.Channels],
					             (audioSource.Channels == 1) ? AudioFormat.Mono16Bit : AudioFormat.Stereo16Bit,
					             (uint)audioSource.Frequency);
					source.QueueAndPlay (wait);
				}
			}

			RequireBuffers (bufferStack, source, 1);
			SourceBuffer buffer = bufferStack.Pop ();

			buffer.Buffer (data, (audioSource.Channels == 1) ? AudioFormat.Mono16Bit : AudioFormat.Stereo16Bit, (uint)audioSource.Frequency);
			source.QueueAndPlay (buffer);
		}

		public IEnumerable<IAudioDevice> GetDevices ()
		{
			return Audio.OpenAL.OpenAL.GetPlaybackDevices().Cast<IAudioDevice>();
		}

		public void FreeSource (AudioSource source)
		{
			buffers.Remove (source);
			pool.FreeSource (source);
		}

		public void Tick()
		{
			pool.Tick();
		}

		public IAudioDevice DefaultDevice
		{
			get { return Audio.OpenAL.OpenAL.GetDefaultPlaybackDevice(); }
		}

		#endregion

		#region IDisposable Members

		public void Dispose ()
		{
			if (this.device != null)
			{
				this.device.Dispose ();
				this.device = null;
			}
		}

		#endregion

		private Context context;
		private PlaybackDevice device;
		private readonly SourcePool<AudioSource> pool = new SourcePool<AudioSource>();
		private readonly object bufferLock = new object ();
		private readonly Dictionary<AudioSource, Stack<SourceBuffer>> buffers = new Dictionary<AudioSource, Stack<SourceBuffer>> ();

		private void RequireBuffers (Stack<SourceBuffer> bufferStack, Source source, int num)
		{
			if (source.ProcessedBuffers > 0)
			{
				SourceBuffer[] freeBuffers = source.Dequeue ();
				for (int i = 0; i < freeBuffers.Length; ++i)
				{
					lock (bufferStack)
					{
						bufferStack.Push (freeBuffers[i]);
					}
				}
			}

			if (bufferStack.Count < num)
				PushBuffers (bufferStack, num);
		}

		private static void PushBuffers (Stack<SourceBuffer> bufferStack, int number)
		{
			SourceBuffer[] sbuffers = SourceBuffer.Generate (number);
			if (sbuffers.Length != number)
				throw new Exception ("Generated buffer count doesn't match requested.");

			for (int i = 0; i < sbuffers.Length; ++i)
				bufferStack.Push (sbuffers[i]);
		}

		private void OnSourceFinished (SourceFinishedEventArgs e)
		{
			var finished = this.SourceFinished;
			if (finished != null)
				finished (this, e);
		}
		
		private void PoolSourceFinished (object sender, SourceFinishedEventArgs<AudioSource> e)
		{
			OnSourceFinished (new SourceFinishedEventArgs (e.Owner));
		}
	}
}