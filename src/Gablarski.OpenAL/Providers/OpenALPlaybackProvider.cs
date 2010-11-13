// Copyright (c) 2010, Eric Maupin
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
using System.ComponentModel.Composition;
using System.Linq;
using Gablarski.Audio;

namespace Gablarski.OpenAL.Providers
{
	[Export (typeof(IAudioPlaybackProvider))]
	public class OpenALPlaybackProvider
		: IAudioPlaybackProvider
	{
		public OpenALPlaybackProvider()
		{
			this.pool.SourceFinished += PoolSourceFinished;
		}

		#region IAudioPlaybackProvider Members
		public event EventHandler<AudioSourceEventArgs> SourceFinished;

		public float Gain
		{
			get { return this.globalGain; }
			set
			{
				if (this.isDisposed)
					throw new ObjectDisposedException ("OpenALPlaybackProvider");

				if (this.globalGain == value)
					return;

				this.globalGain = value;
				RecalculateGains();
			}
		}
		
		public IAudioDevice Device
		{
			get { return this.device; }
			set
			{
				if (this.isDisposed)
					throw new ObjectDisposedException ("OpenALPlaybackProvider");

				this.device = (value as PlaybackDevice);
				if (this.device == null)
					throw new ArgumentException ("Can only accept OpenAL PlaybackDevice devices");
			}
		}
		
		public void SetGain (AudioSource source, float gain)
		{
			if (this.isDisposed)
				throw new ObjectDisposedException ("OpenALPlaybackProvider");
			if (source == null)
				throw new ArgumentNullException ("source");

			var realGains = this.gains.ToDictionary (kvp => kvp.Key, kvp => kvp.Value.Item1);
			realGains[source] = gain;

			RecalculateGains (realGains);
		}

		public void Open()
		{
			if (this.isDisposed)
				throw new ObjectDisposedException ("OpenALPlaybackProvider");
			if (this.device == null)
				throw new InvalidOperationException ("Device is not set");
			if (this.isOpen)
				throw new InvalidOperationException ("Already open");

			OpenALRunner.AddUser();
			OpenALRunner.AddPlaybackProvider (this);

			if (!this.device.IsOpen)
				this.device.Open();

			if (Context.CurrentContext == null || Context.CurrentContext.Device != this.device)
				Context.CreateAndActivate (this.device);

			isOpen = true;
		}

		private bool isOpen;
		public void QueuePlayback (AudioSource audioSource, byte[] data)
		{
			if (this.isDisposed)
				throw new ObjectDisposedException ("OpenALPlaybackProvider");
			if (audioSource == null)
				throw new ArgumentNullException ("audioSource");
			
			Stack<SourceBuffer> bufferStack;
			if (!this.buffers.TryGetValue (audioSource, out bufferStack))
				this.buffers[audioSource] = bufferStack = new Stack<SourceBuffer>();

			lock (this.pool.SyncRoot)
			{
				Source source = this.pool.RequestSource (audioSource);

				Tuple<float, float> gain;
				if (this.gains.TryGetValue (audioSource, out gain))
					source.Gain = gain.Item2;
				else
					source.Gain = this.normalGain;

				const int bufferLen = 6;

				if (data.Length == 0)
					return;

				//if (!source.IsPlaying)
				//{
				//    OpenAL.DebugFormat ("{0} bound to {1} isn't playing, inserting silent buffers", audioSource, source);

				//    RequireBuffers (bufferStack, source, bufferLen);
				//    for (int i = 0; i < bufferLen; ++i)
				//    {
				//        OpenALAudioFormat format = audioSource.ToOpenALFormat();
				//        SourceBuffer wait = bufferStack.Pop();
				//        wait.Buffer (new byte[format.GetBytesPerSample()], format, (uint)audioSource.SampleRate);
				//        source.QueueAndPlay (wait);
				//    }
				//}

				RequireBuffers (bufferStack, source, 1);
				SourceBuffer buffer = bufferStack.Pop();

				buffer.Buffer (data, audioSource.ToOpenALFormat(), (uint)audioSource.SampleRate);
				source.QueueAndPlay (buffer);
			}
		}

		public IEnumerable<IAudioDevice> GetDevices ()
		{
			return OpenAL.GetPlaybackDevices().Cast<IAudioDevice>();
		}

		public void FreeSource (AudioSource source)
		{
			if (this.isDisposed)
				throw new ObjectDisposedException ("OpenALPlaybackProvider");
			if (source == null)
				throw new ArgumentNullException ("source");

			OpenAL.DebugFormat ("Freeing source {0}", source);

			lock (this.gains)
				this.gains = this.gains.Where (kvp => kvp.Key != source).ToDictionary (kvp => kvp.Key, kvp => kvp.Value);

			buffers.Remove (source);
			pool.FreeSource (source);
		}

		public void Tick()
		{
			pool.Tick();
		}

		public IAudioDevice DefaultDevice
		{
			get { return OpenAL.GetDefaultPlaybackDevice(); }
		}

		#endregion

		public override string ToString()
		{
			return "OpenAL Playback";
		}

		#region IDisposable Members

		public void Dispose()
		{
			GC.SuppressFinalize (this);
			Dispose (true);
		}

		~OpenALPlaybackProvider()
		{
			Dispose (false);
		}

		protected void Dispose (bool disposing)
		{
			if (this.isDisposed)
				return;

			OpenAL.DebugFormat ("Freeing OpenALPlaybackProvider. Disposing: ", disposing);

			if (disposing)
			{
				this.pool.Dispose();

				if (this.device != null)
					this.device.Dispose();
			}

			OpenALRunner.RemoveUser();
			OpenALRunner.RemovePlaybackProvider (this);
			this.pool = null;
			this.device = null;
			this.isDisposed = true;
		}

		#endregion

		private bool isDisposed;
		private PlaybackDevice device;
		private float globalGain = 1.0f;
		private float normalGain = 1.0f;
		private SourcePool<AudioSource> pool = new SourcePool<AudioSource>();

		/// <summary>
		/// Gain storage. Tuple Item1 is client gain, Item2 is OpenAL gain.
		/// </summary>
		private Dictionary<AudioSource, Tuple<float, float>> gains = new Dictionary<AudioSource, Tuple<float,float>>();
		private readonly Dictionary<AudioSource, Stack<SourceBuffer>> buffers = new Dictionary<AudioSource, Stack<SourceBuffer>> ();

		private void RecalculateGains (IDictionary<AudioSource, float> realGains)
		{
			lock (this.gains)
			{
				float h = (realGains.Count > 0) ? realGains.Values.Max() : 1.0f;
				float v = this.globalGain;
				float p = 1 / h;

				Dictionary<AudioSource, Tuple<float, float>> newGains = new Dictionary<AudioSource, Tuple<float, float>>();
				foreach (var kvp in realGains)
					newGains[kvp.Key] = new Tuple<float, float> (kvp.Value, kvp.Value * p);

				this.gains = newGains;
				this.normalGain = p;
				Listener.Gain = h * v;
			}
		}

		private void RecalculateGains()
		{
			RecalculateGains (this.gains.ToDictionary (kvp => kvp.Key, kvp => kvp.Value.Item1));
		}

		private static void RequireBuffers (Stack<SourceBuffer> bufferStack, Source source, int num)
		{
			if (source.ProcessedBuffers > 0)
			{
				SourceBuffer[] freeBuffers = source.Dequeue ();
				for (int i = 0; i < freeBuffers.Length; ++i)
				{
					lock (bufferStack)
						bufferStack.Push (freeBuffers[i]);
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

		private void OnSourceFinished (AudioSourceEventArgs e)
		{
			var finished = this.SourceFinished;
			if (finished != null)
				finished (this, e);
		}
		
		private void PoolSourceFinished (object sender, SourceFinishedEventArgs<AudioSource> e)
		{
			OnSourceFinished (new AudioSourceEventArgs (e.Owner));
		}
	}
}